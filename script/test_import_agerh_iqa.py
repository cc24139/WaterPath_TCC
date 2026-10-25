import json
import tempfile
import unittest
from pathlib import Path
from datetime import timezone
from unittest.mock import patch

import import_agerh_iqa as importer


CSV = """station;sample_time;iron;lead;ph\nRDO15;2024-10-15T09:30:00-03:00;420;2,7;7,1\n""".encode()


def mapping(compartment="water"):
    return {
        "source_name": "PMQQS / monitoramento convencional",
        "source_url": "https://monitoramentoriodoce.org/download-dos-dados/",
        "compartment": compartment,
        "timezone": "UTC",
        "columns": {"station": "station", "datetime": "sample_time"},
        "parameters": [{"column": "ph", "code": "Ph", "unit": "pH"}],
        "metals": [
            {"column": "iron", "name": "Fe", "unit": "mg/L", "source_unit": "µg/L"},
            {"column": "lead", "name": "Pb", "unit": "µg/L"},
        ],
        "stations": {
            "RDO15": {
                "body_name": "Rio Doce",
                "location": "Linhares, Espírito Santo",
                "state": "ES",
                "size_km": 10,
                "size_source": "ANA BHO trecho conferido; https://metadados.snirh.gov.br/geonetwork/srv/por/catalog.search#/metadata/32e309da-a8c1-443f-90ac-0cd79ce6a33d",
            }
        },
    }


def validated_records(headers, rows, config):
    # The sandbox's Windows Python lacks the optional IANA tzdata package.
    with patch.object(importer, "ZoneInfo", return_value=timezone.utc):
        return importer.validated_records(headers, rows, config)


class ImporterTests(unittest.TestCase):
    def test_metal_water_values_and_units_are_preserved(self):
        headers, rows = importer.parse_csv(CSV)
        record = validated_records(headers, rows, mapping())[0]
        self.assertEqual(record["metaisPesados"], [
            {"nome": "Fe", "concentracao": 0.42, "unidade": "mg/L"},
            {"nome": "Pb", "concentracao": 2.7, "unidade": "µg/L"},
        ])
        self.assertEqual(record["metaisFonte"][0]["valor"], 420)
        self.assertEqual(record["metaisFonte"][0]["unidade"], "µg/L")

    def test_sediment_cannot_be_sent_through_water_collection_endpoint(self):
        headers, rows = importer.parse_csv(CSV)
        with self.assertRaisesRegex(importer.ImportErrorSafe, "endpoint de coleta representa água"):
            validated_records(headers, rows, mapping("sediment"))

    def test_sediment_rejected_even_without_metal_fields(self):
        headers, rows = importer.parse_csv(CSV)
        config = mapping("sediment")
        config["metals"] = []
        with self.assertRaisesRegex(importer.ImportErrorSafe, "endpoint de coleta representa água"):
            validated_records(headers, rows, config)

    def test_wrong_metal_unit_is_rejected(self):
        headers, rows = importer.parse_csv(CSV)
        config = mapping()
        config["metals"][1]["unit"] = "mg/L"
        with self.assertRaisesRegex(importer.ImportErrorSafe, "Use µg/L"):
            validated_records(headers, rows, config)

    def test_censored_result_is_rejected_instead_of_being_recast_as_numeric(self):
        headers, rows = importer.parse_csv(CSV.replace(b"2,7", b"<2,7"))
        with self.assertRaisesRegex(importer.ImportErrorSafe, "censurado"):
            validated_records(headers, rows, mapping())

    def test_collection_relationships_and_rerun_are_idempotent(self):
        headers, rows = importer.parse_csv(CSV)
        record = validated_records(headers, rows, mapping())[0]
        with tempfile.TemporaryDirectory(dir=Path(__file__).parent) as temp:
            state_path = Path(temp) / "state.json"
            with patch.object(importer, "login", return_value=("test-token", 12)), \
                 patch.object(importer, "create_or_reuse_bodies", return_value={"RDO15": 7}), \
                 patch.object(importer, "existing_signatures", return_value={}), \
                 patch.object(importer, "api_json", return_value=(201, {"id": 99})) as api:
                importer.apply_records("https://api.test", [record], state_path)
                payload = api.call_args.kwargs["payload"]
                self.assertEqual(payload["corpoHidricoId"], 7)
                self.assertEqual(payload["dataHora"], record["dataHora"])
                self.assertEqual(payload["metaisPesados"][0]["nome"], "Fe")
                self.assertEqual(payload["medicoes"][0]["codigoMedicao"], "Ph")
                saved = json.loads(state_path.read_text(encoding="utf-8"))
                self.assertEqual(saved["collections"][record["signature"]]["id"], 99)
                self.assertEqual(saved["collections"][record["signature"]]["compartment"], "water")

                api.reset_mock()
                importer.apply_records("https://api.test", [record], state_path)
                api.assert_not_called()

    def test_quality_only_collection_is_recorded_as_water(self):
        headers, rows = importer.parse_csv(CSV)
        config = mapping()
        config["metals"] = []
        record = validated_records(headers, rows, config)[0]
        self.assertEqual(record["metaisPesados"], [])
        with tempfile.TemporaryDirectory(dir=Path(__file__).parent) as temp:
            state_path = Path(temp) / "state.json"
            with patch.object(importer, "login", return_value=("test-token", 12)), \
                 patch.object(importer, "create_or_reuse_bodies", return_value={"RDO15": 7}), \
                 patch.object(importer, "existing_signatures", return_value={}), \
                 patch.object(importer, "api_json", return_value=(201, {"id": 100})):
                importer.apply_records("https://api.test", [record], state_path)
            saved = json.loads(state_path.read_text(encoding="utf-8"))
            self.assertEqual(saved["collections"][record["signature"]]["compartment"], "water")


if __name__ == "__main__":
    unittest.main()
