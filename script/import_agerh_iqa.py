#!/usr/bin/env python3
"""Preview/import public AGERH IQA-ES CSV data into WaterPath.

CSV column names are intentionally configuration-driven: the public download's
schema can change, so inspect the file before writing a mapping.
"""

from __future__ import annotations

import argparse
import csv
import hashlib
import html.parser
import io
import json
import math
import mimetypes
import os
import re
import sys
import tempfile
import urllib.error
import urllib.parse
import urllib.request
from datetime import datetime, timezone
from zoneinfo import ZoneInfo, ZoneInfoNotFoundError
from pathlib import Path
from typing import Any

PAGE_URL = "https://servicos.agerh.es.gov.br/iqa/home.php"
SOURCE_NAME = "AGERH IQA-ES / Programa QualiRios ES"
USER_AGENT = "WaterPath-AGERH-importer/1.0 (public-data import)"
# Endpoints and request field names are fixed to the current WaterPath API contract.
API_LOGIN_PATH = "/api/user/login"
API_BODY_PATH = "/api/corpohidrico"
API_COLLECTION_PATH = "/api/coleta"
API_COLLECTIONS_FOR_BODY_PATH = "/api/coleta/corpo-hidrico/{body_id}"
API_IMAGE_PATH = "/api/imagem"
API_BODY_FIELDS = {"name": "nome", "location": "localizacao", "size_km": "tamanho",
                   "private": "ehPrivado", "user_ids": "userIds"}
API_COLLECTION_FIELDS = {"body_id": "corpoHidricoId", "datetime": "dataHora", "measurements": "medicoes",
                         "latitude": "latitude", "longitude": "longitude"}
API_MEASUREMENT_FIELDS = {"code": "codigoMedicao", "value": "valor", "unit": "unidade",
                          "censored": "ehCensurado", "limit": "limite"}
API_IMAGE_FIELDS = {"file": "Imagem", "body_id": "IdCorpoHidrico", "collection_id": "idColeta"}
API_MEASUREMENTS = {"Temperatura", "Ph", "OxigenioDissolvido", "Turbidez", "CondutividadeEletrica",
                    "SolidosSuspensosTotais", "Cor", "CarbonoOrganicoTotal", "Dbo5", "DqoPermanganato",
                    "DqoDicromato", "Fosfato", "FosforoTotal", "NitrogenioAmoniacal", "Nitrito", "Nitrato",
                    "NitrogenioTotal", "Iqa"}


class ImportErrorSafe(Exception):
    pass


class CsvLinkParser(html.parser.HTMLParser):
    def __init__(self) -> None:
        super().__init__()
        self.links: list[str] = []

    def handle_starttag(self, tag: str, attrs: list[tuple[str, str | None]]) -> None:
        if tag.lower() != "a":
            return
        href = dict(attrs).get("href")
        if href and re.search(r"\.csv(?:$|[?#])", href, re.IGNORECASE):
            self.links.append(href)


def request(url: str, *, token: str | None = None, data: bytes | None = None,
            content_type: str | None = None, method: str | None = None,
            timeout: int = 45) -> tuple[int, bytes, str]:
    headers = {"User-Agent": USER_AGENT, "Accept": "application/json, text/csv, */*"}
    if token:
        headers["Authorization"] = f"Bearer {token}"
    if content_type:
        headers["Content-Type"] = content_type
    req = urllib.request.Request(url, data=data, headers=headers, method=method)
    try:
        with urllib.request.urlopen(req, timeout=timeout) as response:
            return response.status, response.read(), response.headers.get_content_type()
    except urllib.error.HTTPError as exc:
        # Do not include request headers, request body, or server response (which
        # could echo credentials) in logs or exceptions.
        raise ImportErrorSafe(f"HTTP {exc.code} ao acessar {redact_url(url)}") from None
    except (urllib.error.URLError, TimeoutError) as exc:
        raise ImportErrorSafe(f"Falha de rede ao acessar {redact_url(url)} ({type(exc).__name__}).") from None


def redact_url(url: str) -> str:
    parsed = urllib.parse.urlsplit(url)
    return urllib.parse.urlunsplit((parsed.scheme, parsed.netloc, parsed.path, "", ""))


def discover_csv_url() -> str:
    _, body, _ = request(PAGE_URL)
    parser = CsvLinkParser()
    parser.feed(body.decode("utf-8", errors="replace"))
    links = list(dict.fromkeys(urllib.parse.urljoin(PAGE_URL, href) for href in parser.links))
    if not links:
        raise ImportErrorSafe("A página oficial não apresentou um link CSV reconhecível.")
    if len(links) > 1:
        raise ImportErrorSafe("A página apresentou mais de um link CSV; revisão manual necessária.")
    return links[0]


def fetch_source(path: Path | None) -> tuple[bytes, str]:
    if path:
        return path.read_bytes(), str(path)
    url = discover_csv_url()
    _, body, _ = request(url)
    return body, url


def parse_csv(payload: bytes) -> tuple[list[str], list[dict[str, str]]]:
    try:
        text = payload.decode("utf-8-sig")
    except UnicodeDecodeError:
        raise ImportErrorSafe("CSV não está codificado em UTF-8; converta-o ou revise a fonte.") from None
    try:
        try:
            dialect = csv.Sniffer().sniff(text[:8192], delimiters=";,\t")
        except csv.Error:
            dialect = csv.excel
        parsed = list(csv.reader(io.StringIO(text), dialect))
    except csv.Error as exc:
        raise ImportErrorSafe(f"CSV inválido: {exc}") from None
    if not parsed:
        raise ImportErrorSafe("CSV sem cabeçalhos.")
    normalized_headers = [h.strip() for h in parsed[0]]
    if not all(normalized_headers) or len(set(normalized_headers)) != len(normalized_headers):
        raise ImportErrorSafe("CSV contém cabeçalhos vazios ou duplicados após normalização.")
    normalized_rows: list[dict[str, str]] = []
    for line, values in enumerate(parsed[1:], start=2):
        if len(values) != len(normalized_headers):
            raise ImportErrorSafe(f"CSV malformado na linha {line}: esperadas {len(normalized_headers)} colunas, recebidas {len(values)}.")
        normalized_rows.append(dict(zip(normalized_headers, values)))
    return normalized_headers, normalized_rows


def load_json(path: Path) -> dict[str, Any]:
    try:
        value = json.loads(path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as exc:
        raise ImportErrorSafe(f"Não foi possível ler configuração {path}: {exc}") from None
    if not isinstance(value, dict):
        raise ImportErrorSafe("A configuração deve ser um objeto JSON.")
    return value


def parse_datetime(value: str, timezone_name: str = "America/Sao_Paulo") -> str:
    value = value.strip()
    if re.fullmatch(r"\d{2}/\d{2}/\d{4}|\d{4}-\d{2}-\d{2}", value):
        raise ImportErrorSafe(f"Data {value!r} não contém horário; não será inventado horário de coleta.")
    for fmt in ("%d/%m/%Y %H:%M", "%d/%m/%Y %H:%M:%S", "%Y-%m-%d %H:%M:%S", "%Y-%m-%d %H:%M"):
        try:
            parsed = datetime.strptime(value, fmt)
            return local_to_utc(parsed, timezone_name)
        except ValueError:
            continue
    try:
        parsed = datetime.fromisoformat(value.replace("Z", "+00:00"))
        if parsed.tzinfo is None:
            return local_to_utc(parsed, timezone_name)
        return parsed.astimezone(timezone.utc).isoformat().replace("+00:00", "Z")
    except ZoneInfoNotFoundError:
        raise ImportErrorSafe(f"Fuso horário inválido: {timezone_name!r}.") from None
    except ValueError:
        raise ImportErrorSafe(f"Data/hora não reconhecida: {value!r}; use formato com horário ou ISO-8601.") from None


def local_to_utc(value: datetime, timezone_name: str) -> str:
    zone = ZoneInfo(timezone_name)
    first = value.replace(tzinfo=zone, fold=0)
    second = value.replace(tzinfo=zone, fold=1)
    if first.utcoffset() != second.utcoffset():
        raise ImportErrorSafe(f"Horário local ambíguo por mudança de fuso: {value.isoformat()} ({timezone_name}).")
    utc_value = first.astimezone(timezone.utc)
    if utc_value.astimezone(zone).replace(tzinfo=None) != value:
        raise ImportErrorSafe(f"Horário local inexistente por mudança de fuso: {value.isoformat()} ({timezone_name}).")
    return utc_value.isoformat().replace("+00:00", "Z")


def finite_number(value: str, label: str) -> float:
    try:
        number = float(value.strip().replace(",", "."))
    except (ValueError, AttributeError):
        raise ImportErrorSafe(f"Valor numérico inválido para {label}: {value!r}.") from None
    if not math.isfinite(number):
        raise ImportErrorSafe(f"Valor não finito para {label}.")
    return number


def validated_records(headers: list[str], rows: list[dict[str, str]], mapping: dict[str, Any]) -> list[dict[str, Any]]:
    columns = mapping.get("columns")
    stations = mapping.get("stations")
    if not isinstance(columns, dict) or not isinstance(stations, dict) or not stations:
        raise ImportErrorSafe("Mapeamento incompleto: configure 'columns' e 'stations'.")
    if not all(columns.get(key) for key in ("station", "datetime")):
        raise ImportErrorSafe("Mapeie as colunas obrigatórias 'station' e 'datetime'.")
    source_url = str(mapping.get("source_url", ""))
    source_name = str(mapping.get("source_name", ""))
    parsed_source = urllib.parse.urlsplit(source_url)
    if not source_name or parsed_source.scheme != "https" or not parsed_source.hostname:
        raise ImportErrorSafe("Informe source_name e source_url HTTPS para identificar a origem pública dos dados.")
    timezone_name = str(mapping.get("timezone", "America/Sao_Paulo"))
    try:
        ZoneInfo(timezone_name)
    except ZoneInfoNotFoundError:
        raise ImportErrorSafe(f"Fuso horário inválido: {timezone_name!r}.") from None
    used = {columns.get("station"), columns.get("datetime")}
    for optional in ("latitude", "longitude"):
        if columns.get(optional):
            used.add(columns[optional])
    params = mapping.get("parameters", [])
    if not isinstance(params, list) or not params:
        raise ImportErrorSafe("Configure pelo menos um parâmetro medido.")
    for parameter in params:
        if not all(parameter.get(k) for k in ("column", "code", "unit")):
            raise ImportErrorSafe("Cada parâmetro requer column, code e unit.")
        if parameter["code"] not in API_MEASUREMENTS:
            raise ImportErrorSafe(f"Código {parameter['code']!r} não existe no enum de medições da API.")
        used.add(parameter["column"])
    missing = sorted(str(h) for h in used if h not in headers)
    if missing:
        raise ImportErrorSafe("Cabeçalhos não encontrados no CSV: " + ", ".join(missing))

    records = []
    seen: set[tuple[str, str]] = set()
    for line, row in enumerate(rows, start=2):
        station_key = (row.get(columns["station"]) or "").strip()
        if not station_key:
            continue
        if station_key not in stations:
            raise ImportErrorSafe(f"Ponto {station_key!r} na linha {line} não tem metadados cadastrados.")
        station = stations[station_key]
        required_station = ("body_name", "location", "state", "size_km", "size_source")
        if not all(station.get(k) for k in required_station):
            raise ImportErrorSafe(f"Metadados incompletos para {station_key}: nome, localização, UF e tamanho em km com fonte são obrigatórios.")
        if str(station["state"]).upper() != "ES":
            raise ImportErrorSafe(f"UF {station['state']!r} fora do escopo do Espírito Santo para o ponto {station_key}.")
        size = finite_number(str(station["size_km"]), f"tamanho em km de {station_key}")
        source_text = str(station["size_source"])
        source_urls = re.findall(r"https?://\S+", source_text)
        if (size <= 0 or "BHO" not in source_text.upper()
                or not any(urllib.parse.urlsplit(url.rstrip(".,;)")).hostname == "metadados.snirh.gov.br"
                           for url in source_urls)):
            raise ImportErrorSafe(f"Tamanho de {station_key} deve ser > 0 km e citar BHO com URL do catálogo oficial ANA (metadados.snirh.gov.br).")
        source_time = parse_datetime(row.get(columns["datetime"]) or "", timezone_name)
        parsed_time = datetime.fromisoformat(source_time.replace("Z", "+00:00"))
        if parsed_time > datetime.now(timezone.utc):
            raise ImportErrorSafe(f"Data futura no CSV para {station_key}: {source_time}.")
        signature = (station_key, source_time)
        if signature in seen:
            raise ImportErrorSafe(f"CSV contém ponto/data duplicado: {station_key} {source_time}.")
        seen.add(signature)
        measurements = []
        for parameter in params:
            raw = (row.get(parameter["column"]) or "").strip()
            if not raw or raw.lower() in {"na", "n/a", "null", "-"}:
                continue
            measurements.append({"codigoMedicao": str(parameter["code"]), "valor": finite_number(raw, parameter["code"]),
                                 "unidade": str(parameter["unit"]), "ehCensurado": False, "limite": None})
            value = measurements[-1]["valor"]
            if parameter["code"] == "Ph" and not 0 <= value <= 14:
                raise ImportErrorSafe(f"pH fora do intervalo 0–14 na linha {line}.")
            if parameter["code"] == "Iqa" and not 0 <= value <= 100:
                raise ImportErrorSafe(f"IQA fora do intervalo 0–100 na linha {line}.")
            if parameter["code"] != "Temperatura" and value < 0:
                raise ImportErrorSafe(f"Valor negativo não permitido para {parameter['code']} na linha {line}.")
        if not measurements:
            continue
        record: dict[str, Any] = {"source_station": station_key, "signature": f"{station_key}|{source_time}",
                                  "body": station, "dataHora": source_time, "medicoes": measurements,
                                  "source_url": source_url, "source_name": source_name}
        for key in ("latitude", "longitude"):
            column = columns.get(key)
            if column and (row.get(column) or "").strip():
                coordinate = finite_number(row[column], key)
                lower, upper = (-90, 90) if key == "latitude" else (-180, 180)
                if not lower <= coordinate <= upper:
                    raise ImportErrorSafe(f"{key} fora do intervalo válido na linha {line}.")
                record[key.capitalize()] = coordinate
        records.append(record)
    if not records:
        raise ImportErrorSafe("Nenhuma observação válida com parâmetros foi encontrada.")
    stations_by_body_time: dict[tuple[str, str], set[str]] = {}
    for record in records:
        key = (str(record["body"]["body_name"]).casefold(), record["dataHora"])
        stations_by_body_time.setdefault(key, set()).add(record["source_station"])
    collisions = [key for key, station_keys in stations_by_body_time.items() if len(station_keys) > 1]
    if collisions:
        body_name, timestamp = collisions[0]
        raise ImportErrorSafe(
            f"Estações distintas mapeadas ao mesmo corpo hídrico ({body_name!r}) têm coleta no mesmo horário ({timestamp}); "
            "revise o mapeamento antes de qualquer gravação."
        )
    return records


def api_json(base: str, path: str, *, token: str | None = None, payload: dict[str, Any] | None = None) -> tuple[int, Any]:
    data = json.dumps(payload, ensure_ascii=False).encode("utf-8") if payload is not None else None
    status, body, _ = request(base.rstrip("/") + path, token=token, data=data,
                              content_type="application/json" if data else None,
                              method="POST" if data else "GET")
    try:
        return status, json.loads(body.decode("utf-8")) if body else None
    except (UnicodeDecodeError, json.JSONDecodeError):
        return status, body.decode("utf-8", errors="replace")


def login(base: str) -> tuple[str, int]:
    email = os.environ.get("WATERPATH_EMAIL")
    password = os.environ.get("WATERPATH_PASSWORD")
    if not email or not password:
        raise ImportErrorSafe("Defina WATERPATH_EMAIL e WATERPATH_PASSWORD no ambiente para --apply.")
    _, result = api_json(base, API_LOGIN_PATH, payload={"email": email, "senha": password})
    if not isinstance(result, dict) or not result.get("token") or not result.get("id"):
        raise ImportErrorSafe("Login não retornou token e ID do usuário; confira a API e as credenciais.")
    return str(result["token"]), int(result["id"])


def api_list(value: Any) -> list[dict[str, Any]]:
    if isinstance(value, list):
        return [x for x in value if isinstance(x, dict)]
    if isinstance(value, dict):
        for key in ("$values", "items", "data"):
            if isinstance(value.get(key), list):
                return [x for x in value[key] if isinstance(x, dict)]
    return []


def create_or_reuse_bodies(base: str, token: str, user_id: int, records: list[dict[str, Any]]) -> dict[str, int]:
    # Public list works even when the account has no linked water bodies yet;
    # the per-user endpoint returns 404 for that valid empty state.
    _, existing_raw = api_json(base, API_BODY_PATH, token=token)
    existing = api_list(existing_raw)
    body_ids: dict[str, int] = {}
    body_ids_by_name: dict[str, int] = {}
    for x in existing:
        name = str(x.get("nome", x.get("Nome", ""))).casefold()
        raw_id = x.get("id", x.get("Id"))
        if name and raw_id is not None:
            if name in body_ids_by_name:
                raise ImportErrorSafe(f"A API contém nomes de corpos hídricos ambíguos: {name!r}.")
            body_ids_by_name[name] = int(raw_id)
    for record in records:
        key = record["source_station"]
        if key in body_ids:
            continue
        body = record["body"]
        normalized_name = str(body["body_name"]).casefold()
        if normalized_name in body_ids_by_name:
            match = next(x for x in existing if str(x.get("nome", x.get("Nome", ""))).casefold() == normalized_name)
            location = str(match.get("localizacao", match.get("Localizacao", "")))
            size = match.get("tamanho", match.get("Tamanho"))
            if location != str(body["location"]) or size is None or not math.isclose(
                float(size), float(body["size_km"]), rel_tol=1e-9, abs_tol=1e-6
            ):
                raise ImportErrorSafe(f"Corpo hídrico existente {body['body_name']!r} diverge do mapeamento; revise antes de importar.")
            body_ids[key] = body_ids_by_name[normalized_name]
            continue
        body_payload = {API_BODY_FIELDS["name"]: body["body_name"],
                        API_BODY_FIELDS["location"]: body["location"],
                        API_BODY_FIELDS["size_km"]: float(body["size_km"]),
                        API_BODY_FIELDS["private"]: False, API_BODY_FIELDS["user_ids"]: [user_id]}
        status, _ = api_json(base, API_BODY_PATH, token=token, payload=body_payload)
        if status not in (200, 201):
            raise ImportErrorSafe(f"Falha ao cadastrar corpo hídrico para o ponto {key} (HTTP {status}).")
        _, refreshed = api_json(base, API_BODY_PATH, token=token)
        matched = [x for x in api_list(refreshed) if str(x.get("nome", x.get("Nome", ""))).casefold() == normalized_name]
        if not matched:
            raise ImportErrorSafe(f"Corpo hídrico cadastrado, mas não foi possível confirmar o ID de {key}.")
        if len(matched) > 1:
            raise ImportErrorSafe(f"Cadastro criou nomes duplicados de corpo hídrico {body['body_name']!r}; interrompendo.")
        created_body = matched[0]
        body_id = int(created_body.get("id", created_body.get("Id")))
        body_ids[key] = body_id
        body_ids_by_name[normalized_name] = body_id
        existing.append(created_body)
    return body_ids


def load_state(path: Path) -> dict[str, Any]:
    if not path.exists():
        return {"collections": {}, "images": {}}
    try:
        state = json.loads(path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError):
        raise ImportErrorSafe(f"Estado local inválido: {path}; não será sobrescrito.") from None
    if not isinstance(state, dict):
        raise ImportErrorSafe("Estado local deve ser objeto JSON.")
    state.setdefault("collections", {})
    state.setdefault("images", {})
    return state


def write_state(path: Path, state: dict[str, Any]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    fd, temp_name = tempfile.mkstemp(prefix=path.name, dir=path.parent)
    try:
        with os.fdopen(fd, "w", encoding="utf-8") as stream:
            json.dump(state, stream, ensure_ascii=False, indent=2)
            stream.write("\n")
        os.replace(temp_name, path)
    finally:
        if os.path.exists(temp_name):
            os.unlink(temp_name)


def existing_signatures(base: str, token: str, body_id: int) -> dict[str, list[tuple[float | None, float | None]]]:
    _, response = api_json(base, API_COLLECTIONS_FOR_BODY_PATH.format(body_id=body_id), token=token)
    signatures: dict[str, list[tuple[float | None, float | None]]] = {}
    for row in api_list(response):
        stamp = row.get("dataHora", row.get("DataHora"))
        if stamp:
            key = parse_datetime(str(stamp))
            lat = row.get("latitude", row.get("Latitude"))
            lon = row.get("longitude", row.get("Longitude"))
            signatures.setdefault(key, []).append((float(lat) if lat is not None else None,
                                                   float(lon) if lon is not None else None))
    return signatures


def apply_records(base: str, records: list[dict[str, Any]], state_path: Path) -> None:
    token, user_id = login(base)
    state = load_state(state_path)
    body_ids = create_or_reuse_bodies(base, token, user_id, records)
    # Query existing collections for each body before first write; unknown API
    # response shape is a hard stop rather than a blind duplicate-prone import.
    existing_by_body = {body_id: existing_signatures(base, token, body_id) for body_id in set(body_ids.values())}
    for record in records:
        signature = record["signature"]
        if signature in state["collections"]:
            continue
        body_id = body_ids[record["source_station"]]
        timestamp = record["dataHora"]
        if timestamp in existing_by_body[body_id]:
            lat, lon = record.get("Latitude"), record.get("Longitude")
            coordinates = existing_by_body[body_id][timestamp]
            if lat is not None and lon is not None and (lat, lon) in coordinates:
                print(f"PULADO: já existe coleta no corpo {body_id}, {timestamp}, nas mesmas coordenadas.")
                continue
            raise ImportErrorSafe(f"Colisão ambígua para corpo {body_id} em {timestamp}; ponto {record['source_station']} não pode ser confirmado no banco. Revise manualmente.")
        payload = {API_COLLECTION_FIELDS["body_id"]: body_id,
                   API_COLLECTION_FIELDS["datetime"]: timestamp,
                   API_COLLECTION_FIELDS["measurements"]: record["medicoes"]}
        if "Latitude" in record:
            payload[API_COLLECTION_FIELDS["latitude"]] = record["Latitude"]
        if "Longitude" in record:
            payload[API_COLLECTION_FIELDS["longitude"]] = record["Longitude"]
        try:
            _, created = api_json(base, API_COLLECTION_PATH, token=token, payload=payload)
        except ImportErrorSafe as exc:
            if "HTTP 409" in str(exc):
                print(f"PULADO: conflito já existente para {record['source_station']} {timestamp}.")
                continue
            raise
        collection_id = created.get("id", created.get("Id")) if isinstance(created, dict) else None
        if not collection_id:
            raise ImportErrorSafe("API não retornou ID após criar coleta; interrompendo para evitar repetição incerta.")
        state["collections"][signature] = {"id": collection_id, "body_id": body_id, "timestamp": timestamp,
                                            "station": record["source_station"], "body_name": record["body"]["body_name"],
                                            "source_url": record["source_url"], "source_name": record["source_name"]}
        write_state(state_path, state)
        existing_by_body[body_id].setdefault(timestamp, []).append((record.get("Latitude"), record.get("Longitude")))
        print(f"Importada coleta {collection_id} do ponto {record['source_station']}.")


def upload_images(base: str, token: str, manifest_path: Path, state_path: Path) -> None:
    manifest = load_json(manifest_path)
    entries = manifest.get("images")
    if not isinstance(entries, list):
        raise ImportErrorSafe("Manifesto de imagens precisa conter lista 'images'.")
    state = load_state(state_path)
    for item in entries:
        required = ("file", "source", "license", "attribution", "station", "source_id", "signature")
        if not all(item.get(key) for key in required):
            raise ImportErrorSafe("Cada imagem exige arquivo, origem, licença, atribuição, body_id, station e source_id.")
        path = Path(item["file"]).expanduser().resolve()
        if not path.is_file():
            raise ImportErrorSafe(f"Arquivo de imagem não encontrado: {path}")
        digest = hashlib.sha256(path.read_bytes()).hexdigest()
        key = str(item["source_id"])
        signature = str(item["signature"])
        collection = state["collections"].get(signature)
        if not collection:
            raise ImportErrorSafe(f"Imagem {key} não aponta para coleta importada no estado local: {signature}.")
        if collection.get("station") != item["station"]:
            raise ImportErrorSafe(f"A estação no manifesto da imagem {key} não corresponde à coleta {signature}.")
        duplicate_hash = next((saved_key for saved_key, saved in state["images"].items()
                               if isinstance(saved, dict) and saved.get("sha256") == digest), None)
        if key in state["images"] or duplicate_hash:
            print(f"PULADO: imagem já registrada ({key}).")
            state["images"].setdefault(key, {"sha256": digest, "duplicate_of": duplicate_hash,
                                              "station": item["station"], "signature": signature})
            write_state(state_path, state)
            continue
        boundary = "----WaterPath" + hashlib.sha256((digest + key).encode()).hexdigest()[:24]
        filename = path.name.replace('"', "")
        mime = mimetypes.guess_type(filename)[0] or "application/octet-stream"
        # These are the fields accepted by CadastrarImagemDTO. Source/license/
        # attribution remain in the local audit state and manifest.
        fields = {API_IMAGE_FIELDS["body_id"]: str(collection["body_id"]),
                  API_IMAGE_FIELDS["collection_id"]: str(collection["id"])}
        chunks = []
        for name, value in fields.items():
            chunks.append(f"--{boundary}\r\nContent-Disposition: form-data; name=\"{name}\"\r\n\r\n{value}\r\n".encode())
        chunks.append(f"--{boundary}\r\nContent-Disposition: form-data; name=\"{API_IMAGE_FIELDS['file']}\"; filename=\"{filename}\"\r\nContent-Type: {mime}\r\n\r\n".encode())
        chunks.append(path.read_bytes())
        chunks.append(f"\r\n--{boundary}--\r\n".encode())
        status, _, _ = request(base.rstrip("/") + API_IMAGE_PATH, token=token,
                               data=b"".join(chunks), content_type=f"multipart/form-data; boundary={boundary}")
        if status not in (200, 201):
            raise ImportErrorSafe(f"Falha ao enviar imagem {key} (HTTP {status}).")
        state["images"][key] = {"sha256": digest, "source": item["source"], "license": item["license"],
                                 "attribution": item["attribution"], "station": item["station"],
                                 "signature": signature}
        write_state(state_path, state)
        print(f"Imagem enviada: {key}.")


def validate_image_manifest(manifest_path: Path, records: list[dict[str, Any]]) -> None:
    manifest = load_json(manifest_path)
    entries = manifest.get("images")
    if not isinstance(entries, list):
        raise ImportErrorSafe("Manifesto de imagens precisa conter lista 'images'.")
    signatures = {record["signature"]: record["source_station"] for record in records}
    seen_ids: set[str] = set()
    for item in entries:
        required = ("file", "source", "license", "attribution", "station", "source_id", "signature")
        if not all(item.get(key) for key in required):
            raise ImportErrorSafe("Cada imagem exige file, source (URL), license, attribution, station, source_id e signature.")
        key = str(item["source_id"])
        if key in seen_ids:
            raise ImportErrorSafe(f"source_id duplicado no manifesto de imagens: {key}.")
        seen_ids.add(key)
        path = Path(item["file"]).expanduser()
        if not path.is_file():
            raise ImportErrorSafe(f"Arquivo de imagem não encontrado: {path}")
        signature = str(item["signature"])
        if signatures.get(signature) != item["station"]:
            raise ImportErrorSafe(f"Imagem {key} não corresponde a uma linha validada do CSV (signature/station).")


def main() -> int:
    parser = argparse.ArgumentParser(description="Pré-visualiza ou importa o CSV público IQA-ES da AGERH.")
    parser.add_argument("--csv", type=Path, help="CSV local; sem esta opção baixa o link CSV atual da página AGERH.")
    parser.add_argument("--mapping", type=Path, help="JSON com mapeamento explícito dos cabeçalhos e pontos.")
    parser.add_argument("--base-url", default=os.environ.get("WATERPATH_API_URL"), help="URL base da API (ou WATERPATH_API_URL).")
    parser.add_argument("--state", type=Path,
                        default=Path(__file__).resolve().parent / ".waterpath-import-state.json",
                        help="Estado local para repetição segura.")
    parser.add_argument("--images-manifest", type=Path, help="Manifesto explícito de imagens licenciadas.")
    parser.add_argument("--apply", action="store_true", help="Executa gravações; padrão é somente prévia.")
    args = parser.parse_args()
    try:
        payload, origin = fetch_source(args.csv)
        headers, rows = parse_csv(payload)
        print(f"Fonte: {origin}\nCabeçalhos ({len(headers)}): {json.dumps(headers, ensure_ascii=False)}\nLinhas: {len(rows)}")
        if not args.mapping:
            print("Somente inspeção: forneça --mapping para validar observações. Nada foi gravado.")
            return 0
        mapping = load_json(args.mapping)
        source_url = urllib.parse.urlsplit(str(mapping.get("source_url", "")))
        if not args.csv and source_url.hostname != urllib.parse.urlsplit(origin).hostname:
            raise ImportErrorSafe("source_url do mapeamento não corresponde ao domínio do CSV oficial baixado.")
        print(f"Origem declarada no mapeamento: {mapping.get('source_name', '')} — {mapping.get('source_url', '')}")
        records = validated_records(headers, rows, mapping)
        if args.images_manifest:
            validate_image_manifest(args.images_manifest, records)
        print(f"Observações válidas: {len(records)}; pontos: {len({r['source_station'] for r in records})}")
        for record in records[:10]:
            print(f"  {record['source_station']} | {record['dataHora']} | {len(record['medicoes'])} medições")
        if len(records) > 10:
            print(f"  ... mais {len(records) - 10}")
        if not args.apply:
            print("Prévia concluída. Nada foi gravado; use --apply após revisar o mapeamento e a prévia.")
            return 0
        if not args.base_url:
            raise ImportErrorSafe("Defina --base-url ou WATERPATH_API_URL para --apply.")
        apply_records(args.base_url, records, args.state)
        if args.images_manifest:
            token, _ = login(args.base_url)
            upload_images(args.base_url, token, args.images_manifest, args.state)
        return 0
    except (ImportErrorSafe, OSError) as exc:
        print(f"ERRO: {exc}", file=sys.stderr)
        return 2


if __name__ == "__main__":
    raise SystemExit(main())
