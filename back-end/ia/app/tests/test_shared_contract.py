"""Impede divergencia entre FastAPI/Pydantic e o contrato consumido pela API C#."""
import json
import math
from pathlib import Path
import unittest
from unittest.mock import patch

import numpy as np

from DTOs.Amostra import Amostra
from DTOs.CollectionContext import Collection, CollectionContext, Measurement, MetalObservation
from main import analyze_image, app
from services.risk.classification import HistoryItem


CONTRACT_PATH = Path(__file__).resolve().parents[3] / "contracts/ia/v1/openapi.json"


class SharedContractTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.contract = json.loads(CONTRACT_PATH.read_text(encoding="utf-8"))
        cls.schemas = cls.contract["components"]["schemas"]

    def resolve(self, schema):
        if "$ref" not in schema:
            return schema
        return self.schemas[schema["$ref"].rsplit("/", 1)[-1]]

    def assert_matches(self, value, schema, path="$"):
        schema = self.resolve(schema)
        if "anyOf" in schema:
            errors = []
            for option in schema["anyOf"]:
                try:
                    self.assert_matches(value, option, path)
                    return
                except AssertionError as error:
                    errors.append(str(error))
            self.fail(f"{path} nao corresponde a anyOf: {errors}")
        if "const" in schema:
            self.assertEqual(value, schema["const"], path)
        if "enum" in schema:
            self.assertIn(value, schema["enum"], path)
        types = schema.get("type")
        if types:
            types = [types] if isinstance(types, str) else types
            valid = ((value is None and "null" in types)
                     or (isinstance(value, bool) and "boolean" in types)
                     or (isinstance(value, int) and not isinstance(value, bool) and "integer" in types)
                     or (isinstance(value, (int, float)) and not isinstance(value, bool) and "number" in types
                         and math.isfinite(value))
                     or (isinstance(value, str) and "string" in types)
                     or (isinstance(value, list) and "array" in types)
                     or (isinstance(value, dict) and "object" in types))
            self.assertTrue(valid, f"{path}: tipo invalido para {types}: {value!r}")
        if isinstance(value, (int, float)) and not isinstance(value, bool):
            if "minimum" in schema:
                self.assertGreaterEqual(value, schema["minimum"], path)
            if "maximum" in schema:
                self.assertLessEqual(value, schema["maximum"], path)
        if isinstance(value, str) and "minLength" in schema:
            self.assertGreaterEqual(len(value), schema["minLength"], path)
        if isinstance(value, list):
            if "maxItems" in schema:
                self.assertLessEqual(len(value), schema["maxItems"], path)
            for index, item in enumerate(value):
                self.assert_matches(item, schema.get("items", {}), f"{path}[{index}]")
        if isinstance(value, dict):
            properties = schema.get("properties", {})
            self.assertTrue(set(schema.get("required", [])) <= set(value), f"{path}: campo obrigatorio ausente")
            if schema.get("additionalProperties") is False:
                self.assertEqual(set(), set(value) - set(properties), f"{path}: campo fora do contrato")
            for name, item in value.items():
                if name in properties:
                    self.assert_matches(item, properties[name], f"{path}.{name}")

    def test_pydantic_models_match_canonical_input_schemas(self):
        cases = [
            (Amostra, "Sample"), (HistoryItem, "RiskHistoryItem"),
            (Measurement, "Measurement"), (MetalObservation, "MetalObservation"),
            (Collection, "Collection"), (CollectionContext, "CollectionContext"),
        ]
        for model, schema_name in cases:
            with self.subTest(schema=schema_name):
                generated = model.model_json_schema(by_alias=True)
                canonical = self.schemas[schema_name]
                self.assertEqual(set(canonical["properties"]), set(generated["properties"]))
                self.assertEqual(set(canonical.get("required", [])), set(generated.get("required", [])))
                self.assertIs(generated.get("additionalProperties"), False)

        sample = self.schemas["Sample"]["example"]
        context = self.schemas["CollectionContext"]["example"]
        Amostra.model_validate(sample)
        CollectionContext.model_validate(context)
        self.assert_matches(sample, self.schemas["Sample"])
        self.assert_matches(context, self.schemas["CollectionContext"])

        for name in self.schemas["Sample"]["required"]:
            invalid = dict(sample)
            invalid.pop(name)
            with self.subTest(missing=name), self.assertRaises(ValueError):
                Amostra.model_validate(invalid)
        with self.assertRaises(ValueError):
            Amostra.model_validate({**sample, "campo_fora_do_contrato": 1})
        for name, field in self.schemas["Sample"]["properties"].items():
            if "minimum" in field:
                with self.subTest(minimum=name), self.assertRaises(ValueError):
                    Amostra.model_validate({**sample, name: field["minimum"] - 1})
            if "maximum" in field:
                with self.subTest(maximum=name), self.assertRaises(ValueError):
                    Amostra.model_validate({**sample, name: field["maximum"] + 1})
        with self.assertRaises(ValueError):
            Amostra.model_validate({**sample, "metais_pesados": [{"name": "Hg", "value": 1, "unit": "µg/L"}]})

    def test_fastapi_multipart_matches_canonical_route(self):
        generated = app.openapi()["paths"]["/analyze"]["post"]["requestBody"]["content"]["multipart/form-data"]["schema"]
        if "$ref" in generated:
            generated = app.openapi()["components"]["schemas"][generated["$ref"].rsplit("/", 1)[-1]]
        canonical = self.contract["paths"]["/analyze"]["post"]["requestBody"]["content"]["multipart/form-data"]["schema"]
        self.assertEqual(set(canonical["properties"]), set(generated["properties"]))
        self.assertEqual(set(canonical["required"]), set(generated["required"]))

    def test_real_response_builder_matches_canonical_response(self):
        sample = self.schemas["Sample"]["example"]
        context = self.schemas["CollectionContext"]["example"]
        with patch("main.vision.read_image", return_value=np.zeros((10, 10, 3), dtype=np.uint8)), \
             patch("main.vision.load_model", return_value=object()), \
             patch("main.vision.predict_image", return_value=[]), \
             patch("main.vision.class_names", return_value=[]), \
             patch("main.vision.annotate_image", return_value=b"jpeg"), \
             patch("main.vision.model_version", return_value="vision-test"), \
             patch("main.metals.predict_metals", return_value=[]), \
             patch("main.metals.model_metadata", return_value={"version": "metals-test"}):
            result = analyze_image(object(), json.dumps(sample), include_annotation=True,
                                   collection_context=json.dumps(context))
        self.assert_matches(result, self.schemas["AnalyzeResponse"])
        self.assert_matches(self.schemas["AnalyzeResponse"]["example"], self.schemas["AnalyzeResponse"])


if __name__ == "__main__":
    unittest.main()
