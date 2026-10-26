#!/usr/bin/env python3
"""Preview/import a documented long-form WaterPath CSV (features and expected targets)."""
from __future__ import annotations

import argparse
import csv
import json
import math
import os
import sys
from collections import defaultdict
from datetime import datetime, timezone
from pathlib import Path
from typing import Any

import import_agerh_iqa as api

HEADER = ["amostra_id", "origem", "sintetico", "corpo_hidrico", "ponto", "data_hora",
          "latitude", "longitude", "profundidade_m", "papel", "destino", "parametro", "valor", "unidade"]
FEATURES = {"Ph": "pH", "OxigenioDissolvido": "mg/L", "Turbidez": "NTU", "Iqa": "pontos"}
TARGETS = {"Fe": "mg/L", "Mn": "mg/L", "Cr": "µg/L", "Ni": "µg/L", "Cu": "µg/L",
           "Zn": "µg/L", "Cd": "µg/L", "Pb": "µg/L"}
OPERATIONAL = {"Ph", "OxigenioDissolvido", "Turbidez", "Iqa"}
METALS = {"Fe", "Mn", "Cr", "Ni", "Cu", "Zn", "Cd", "Pb"}


def fail(line: int, column: str, message: str) -> None:
    raise api.ImportErrorSafe(f"linha {line}, coluna {column}: {message}")


def number(row: dict[str, str], line: int, key: str, *, required: bool = True) -> float | None:
    raw = row.get(key, "").strip()
    if not raw and not required:
        return None
    try:
        value = float(raw.replace(",", "."))
    except ValueError:
        fail(line, key, "número inválido.")
    if not math.isfinite(value):
        fail(line, key, "o número deve ser finito.")
    return value


def read_csv(path: Path) -> tuple[list[str], list[dict[str, str]], list[int]]:
    try:
        text = path.read_text(encoding="utf-8-sig")
    except (OSError, UnicodeError) as exc:
        raise api.ImportErrorSafe(f"Não foi possível ler o CSV {path}: {exc}") from None
    try:
        dialect = csv.Sniffer().sniff(text[:8192], delimiters=";,\t")
        records = list(csv.reader(text.splitlines(), dialect))
    except csv.Error as exc:
        raise api.ImportErrorSafe(f"CSV inválido: {exc}") from None
    if not records:
        raise api.ImportErrorSafe("CSV vazio.")
    header = [x.strip() for x in records[0]]
    if header != HEADER:
        raise api.ImportErrorSafe("Cabeçalho inválido. Esperado exatamente: " + ",".join(HEADER))
    rows: list[dict[str, str]] = []
    lines: list[int] = []
    for line, values in enumerate(records[1:], 2):
        if not values or all(not v.strip() for v in values):
            continue
        if len(values) != len(header):
            raise api.ImportErrorSafe(f"linha {line}, coluna CSV: esperadas {len(header)} colunas, recebidas {len(values)}.")
        rows.append(dict(zip(header, values)))
        lines.append(line)
    return header, rows, lines


def validate(rows: list[dict[str, str]], lines: list[int]) -> list[dict[str, Any]]:
    samples: dict[str, dict[str, Any]] = {}
    for row, line in zip(rows, lines):
        for key in HEADER:
            if not row[key].strip():
                fail(line, key, "campo obrigatório vazio.")
        sample_id = row["amostra_id"].strip()
        origin = row["origem"].strip()
        if row["sintetico"].strip().lower() not in {"true", "false"}:
            fail(line, "sintetico", "use true ou false.")
        synthetic = row["sintetico"].strip().lower() == "true"
        if row["papel"].strip() not in {"feature", "target"}:
            fail(line, "papel", "use feature ou target.")
        role, destination = row["papel"].strip(), row["destino"].strip()
        if role == "feature" and destination != "operacional":
            fail(line, "destino", "features devem usar operacional.")
        if role == "target" and destination != "comparacao":
            fail(line, "destino", "targets devem usar comparacao.")
        parameter, unit = row["parametro"].strip(), row["unidade"].strip()
        allowed = FEATURES if role == "feature" else TARGETS
        if parameter not in allowed:
            fail(line, "parametro", f"parâmetro não permitido para papel {role}: {parameter!r}.")
        if unit != allowed[parameter]:
            fail(line, "unidade", f"unidade incompatível para {parameter}; use {allowed[parameter]}.")
        value = number(row, line, "valor")
        if role == "feature":
            if parameter == "Ph" and not 0 <= value <= 14:
                fail(line, "valor", "pH deve estar entre 0 e 14.")
            if parameter == "Iqa" and not 0 <= value <= 100:
                fail(line, "valor", "IQA deve estar entre 0 e 100.")
            if parameter in {"OxigenioDissolvido", "Turbidez"} and value < 0:
                fail(line, "valor", "não pode ser negativo.")
        elif value < 0:
            fail(line, "valor", "concentração não pode ser negativa.")
        stamp_raw = row["data_hora"].strip()
        try:
            stamp = datetime.fromisoformat(stamp_raw.replace("Z", "+00:00"))
            if stamp.tzinfo is None or stamp.utcoffset() is None:
                fail(line, "data_hora", "informe data/hora ISO-8601 com fuso.")
            if stamp > datetime.now(stamp.tzinfo):
                fail(line, "data_hora", "data/hora futura não permitida pela API.")
            stamp_utc = stamp.astimezone(timezone.utc).isoformat().replace("+00:00", "Z")
        except ValueError:
            fail(line, "data_hora", "data/hora inválida; use ISO-8601 com fuso.")
        lat, lon = number(row, line, "latitude"), number(row, line, "longitude")
        depth = number(row, line, "profundidade_m")
        if not -90 <= lat <= 90:
            fail(line, "latitude", "deve estar entre -90 e 90.")
        if not -180 <= lon <= 180:
            fail(line, "longitude", "deve estar entre -180 e 180.")
        if depth < 0:
            fail(line, "profundidade_m", "não pode ser negativa.")
        metadata = {"origin": origin, "synthetic": synthetic, "body": row["corpo_hidrico"].strip(),
                    "point": row["ponto"].strip(), "timestamp": stamp_utc, "lat": lat, "lon": lon, "depth": depth}
        if sample_id not in samples:
            samples[sample_id] = {**metadata, "features": {}, "targets": {}, "line": line}
        sample = samples[sample_id]
        if any(sample[k] != v for k, v in metadata.items()):
            fail(line, "amostra_id", f"metadados divergem das outras linhas da amostra {sample_id!r}.")
        group = "features" if role == "feature" else "targets"
        if parameter in sample[group]:
            fail(line, "parametro", f"parâmetro duplicado na amostra {sample_id!r}.")
        sample[group][parameter] = {"value": value, "unit": unit, "line": line}
    if not samples:
        raise api.ImportErrorSafe("CSV sem linhas de dados.")
    for sample_id, sample in samples.items():
        if not sample["features"]:
            fail(sample["line"], "papel", f"amostra {sample_id!r} não contém feature operacional.")
    return [{"sample_id": key, **value} for key, value in samples.items()]


def apply(base: str, records: list[dict[str, Any]], state_path: Path) -> None:
    token, user_id = api.login(base)
    state = api.load_state(state_path)
    state.setdefault("csv_samples", {})
    existing_raw_status, existing_raw = api.api_json(base, api.API_BODY_PATH, token=token)
    existing = api.api_list(existing_raw)
    by_name = {str(item.get("nome", item.get("Nome", ""))).casefold(): int(item.get("id", item.get("Id")))
               for item in existing if item.get("id", item.get("Id")) is not None}
    body_ids: dict[str, int] = {}
    for record in records:
        name = record["body"]
        if name not in body_ids:
            if name.casefold() in by_name:
                existing_body = next(x for x in existing
                                     if str(x.get("nome", x.get("Nome", ""))).casefold() == name.casefold())
                location = str(existing_body.get("localizacao", existing_body.get("Localizacao", "")))
                size = existing_body.get("tamanho", existing_body.get("Tamanho"))
                if location != record["point"] or size is None or not math.isclose(
                        float(size), 0.01, rel_tol=1e-9, abs_tol=1e-6):
                    raise api.ImportErrorSafe(
                        f"Corpo hídrico existente {name!r} diverge da localização/tamanho fictício esperado; revise antes de importar.")
                body_ids[name] = by_name[name.casefold()]
            else:
                # The source CSV has no measured reach length; 0.01 km is a
                # documented synthetic placeholder for this synthetic fixture.
                payload = {"nome": name, "localizacao": record["point"], "tamanho": 0.01,
                           "ehPrivado": False, "userIds": [user_id]}
                _, result = api.api_json(base, api.API_BODY_PATH, token=token, payload=payload)
                raw_id = result.get("id", result.get("Id")) if isinstance(result, dict) else None
                if not raw_id:
                    _, refreshed = api.api_json(base, api.API_BODY_PATH, token=token)
                    matches = [x for x in api.api_list(refreshed) if str(x.get("nome", x.get("Nome", ""))).casefold() == name.casefold()]
                    if len(matches) != 1:
                        raise api.ImportErrorSafe(f"Corpo hídrico {name!r} não confirmado após criação; interrompendo.")
                    raw_id = matches[0].get("id", matches[0].get("Id"))
                body_ids[name] = int(raw_id)
    for record in records:
        sample_id = record["sample_id"]
        body_id = body_ids[record["body"]]
        timestamp = record["timestamp"]
        saved = state["csv_samples"].get(sample_id, {})
        collection_id = saved.get("collection_id")
        if not collection_id:
            _, existing_response = api.api_json(base, api.API_COLLECTIONS_FOR_BODY_PATH.format(body_id=body_id), token=token)
            matches = []
            for row in api.api_list(existing_response):
                stamp = row.get("dataHora", row.get("DataHora"))
                lat = row.get("latitude", row.get("Latitude"))
                lon = row.get("longitude", row.get("Longitude"))
                if stamp and api.parse_datetime(str(stamp)) == timestamp and lat is not None and lon is not None \
                        and math.isclose(float(lat), record["lat"], abs_tol=1e-7) \
                        and math.isclose(float(lon), record["lon"], abs_tol=1e-7):
                    matches.append(row)
            if len(matches) > 1:
                raise api.ImportErrorSafe(f"Mais de uma coleta corresponde a {sample_id}; revisão manual necessária.")
            if matches:
                collection_id = matches[0].get("id", matches[0].get("Id"))
                if not collection_id:
                    raise api.ImportErrorSafe(f"Coleta existente de {sample_id} não expõe ID; não é seguro retomar.")
                print(f"Retomada: coleta existente {collection_id} encontrada para {sample_id}.")
            else:
                measurements = [{"codigoMedicao": parameter, "valor": item["value"], "unidade": item["unit"],
                                 "ehCensurado": False, "limite": None}
                                for parameter, item in record["features"].items()]
                payload = {"corpoHidricoId": body_id, "dataHora": timestamp, "medicoes": measurements,
                           "latitude": record["lat"], "longitude": record["lon"], "profundidadeMetros": record["depth"]}
                _, created = api.api_json(base, api.API_COLLECTION_PATH, token=token, payload=payload)
                collection_id = created.get("id", created.get("Id")) if isinstance(created, dict) else None
                if not collection_id:
                    raise api.ImportErrorSafe(f"API não confirmou ID da coleta {sample_id}; interrompendo antes de comparar.")
                print(f"Coleta operacional {collection_id} criada para {sample_id}.")
            state["csv_samples"][sample_id] = {"collection_id": int(collection_id), "body_id": body_id, "timestamp": timestamp}
            api.write_state(state_path, state)
        if not record["targets"]:
            print(f"Amostra {sample_id}: 0 targets esperados.")
            continue
        status, saved_raw, _ = api.request(
            base.rstrip("/") + "/api/comparacoes-esperadas?coletaId=" + str(collection_id), token=token)
        try:
            saved_values = api.api_list(json.loads(saved_raw.decode("utf-8"))) if saved_raw else []
        except (UnicodeDecodeError, json.JSONDecodeError):
            raise api.ImportErrorSafe(f"Resposta de comparações inválida ao retomar {sample_id}; nada será ignorado.") from None
        if status >= 400:
            raise api.ImportErrorSafe(f"Falha ao consultar comparações existentes de {sample_id} (HTTP {status}).")
        saved_by_key = {(str(x.get("amostraId", x.get("AmostraId", ""))),
                         str(x.get("parametro", x.get("Parametro", "")))): x for x in saved_values}
        for parameter, item in record["targets"].items():
            prior = saved_by_key.get((sample_id, parameter))
            if prior:
                expected = prior.get("valorEsperado", prior.get("ValorEsperado"))
                unit = prior.get("unidade", prior.get("Unidade"))
                if not math.isclose(float(expected), item["value"], rel_tol=1e-10, abs_tol=1e-12) or unit != item["unit"]:
                    raise api.ImportErrorSafe(f"Valor esperado já existe e diverge para {sample_id}/{parameter}.")
                print(f"PULADO: valor esperado já confirmado ({sample_id}/{parameter}).")
                continue
            body = {"coletaId": int(collection_id), "amostraId": sample_id, "parametro": parameter,
                    "valorEsperado": item["value"], "unidade": item["unit"], "origem": record["origin"],
                    "sintetico": record["synthetic"]}
            api.api_json(base, "/api/comparacoes-esperadas", token=token, payload=body)
            print(f"Valor esperado salvo: {sample_id}/{parameter}.")


def main() -> int:
    parser = argparse.ArgumentParser(description="Pré-visualiza/importa CSV local WaterPath no contrato documentado.")
    parser.add_argument("--csv", type=Path, default=Path(__file__).with_name("sinteticos.csv"), help="CSV local que segue o contrato documentado.")
    parser.add_argument("--base-url", default=os.environ.get("WATERPATH_API_URL"))
    parser.add_argument("--state", type=Path, default=Path(__file__).with_name(".waterpath-csv-import-state.json"))
    parser.add_argument("--apply", action="store_true", help="Grava; sem esta opção executa somente validação/prévia.")
    args = parser.parse_args()
    try:
        if args.csv.suffix.lower() != ".csv":
            raise api.ImportErrorSafe("O arquivo de entrada deve ter extensão .csv.")
        _, rows, lines = read_csv(args.csv)
        records = validate(rows, lines)
        print(f"CSV válido: {len(rows)} linhas, {len(records)} amostras; features e targets roteados separadamente.")
        for record in records:
            print(f"  {record['sample_id']} | {record['body']} / {record['point']} | {record['timestamp']} | features={len(record['features'])} targets={len(record['targets'])}")
        if not args.apply:
            print("Prévia concluída. Nada foi gravado; use --apply após revisar.")
            return 0
        if not args.base_url:
            raise api.ImportErrorSafe("Defina --base-url ou WATERPATH_API_URL para --apply.")
        apply(args.base_url, records, args.state)
        return 0
    except (api.ImportErrorSafe, OSError) as exc:
        print(f"ERRO: {exc}", file=sys.stderr)
        return 2


if __name__ == "__main__":
    raise SystemExit(main())
