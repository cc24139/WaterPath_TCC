import sys
import json
import unittest
from pathlib import Path
from unittest.mock import patch

sys.path.insert(0, str(Path(__file__).parent))
import script as importer


class LocalCsvTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        _, cls.rows, cls.lines = importer.read_csv(Path(__file__).with_name("sinteticos.csv"))
        cls.samples = importer.validate(cls.rows, cls.lines)

    def test_features_and_expected_targets_are_separate(self):
        self.assertEqual(len(self.samples), 2)
        self.assertEqual(set(self.samples[0]["features"]), {"Ph", "OxigenioDissolvido", "Turbidez", "Iqa"})
        self.assertEqual(set(self.samples[0]["targets"]), {"Fe", "Pb"})
        self.assertNotIn("Fe", self.samples[0]["features"])

    def test_units_and_values_preserved(self):
        self.assertEqual(self.samples[0]["features"]["Ph"]["unit"], "pH")
        self.assertEqual(self.samples[0]["targets"]["Pb"]["unit"], "µg/L")
        self.assertEqual(self.samples[1]["targets"]["Fe"]["value"], 0.16)

    def test_role_destination_mismatch_reports_line_and_column(self):
        rows = [dict(row) for row in self.rows]
        rows[0]["destino"] = "comparacao"
        with self.assertRaisesRegex(importer.api.ImportErrorSafe, r"linha 2, coluna destino"):
            importer.validate(rows, self.lines)

    def test_bad_unit_reports_line_and_column(self):
        rows = [dict(row) for row in self.rows]
        rows[0]["unidade"] = "mg/L"
        with self.assertRaisesRegex(importer.api.ImportErrorSafe, r"linha 2, coluna unidade"):
            importer.validate(rows, self.lines)

    def test_non_finite_number_reports_line_and_column(self):
        rows = [dict(row) for row in self.rows]
        rows[0]["valor"] = "NaN"
        with self.assertRaisesRegex(importer.api.ImportErrorSafe, r"linha 2, coluna valor"):
            importer.validate(rows, self.lines)

    def test_target_cannot_be_routed_to_operational_table(self):
        rows = [dict(row) for row in self.rows]
        rows[4]["destino"] = "operacional"
        with self.assertRaisesRegex(importer.api.ImportErrorSafe, r"linha 6, coluna destino"):
            importer.validate(rows, self.lines)

    def test_apply_rerun_recovers_collection_and_skips_expected_values(self):
        record = self.samples[0]
        existing_body = {"id": 7, "nome": record["body"], "localizacao": record["point"], "tamanho": 0.01}
        existing_collection = {"id": 42, "dataHora": record["timestamp"],
                               "latitude": record["lat"], "longitude": record["lon"]}
        expected = [{"amostraId": record["sample_id"], "parametro": parameter,
                     "valorEsperado": item["value"], "unidade": item["unit"]}
                    for parameter, item in record["targets"].items()]

        def api_json(_base, path, *, token=None, payload=None):
            if path == importer.api.API_BODY_PATH:
                return 200, [existing_body]
            if path == importer.api.API_COLLECTIONS_FOR_BODY_PATH.format(body_id=7):
                return 200, [existing_collection]
            raise AssertionError(f"Unexpected write or API route: {path}")

        def request(_url, *, token=None):
            return 200, json.dumps(expected).encode(), "application/json"

        with patch.object(importer.api, "login", return_value=("token", 3)), \
             patch.object(importer.api, "load_state", return_value={"collections": {}, "images": {}}), \
             patch.object(importer.api, "api_json", side_effect=api_json), \
             patch.object(importer.api, "request", side_effect=request), \
             patch.object(importer.api, "write_state") as write_state:
            importer.apply("https://example.test", [record], Path("state.json"))
            importer.apply("https://example.test", [record], Path("state.json"))
        self.assertEqual(write_state.call_count, 1)


if __name__ == "__main__":
    unittest.main()
