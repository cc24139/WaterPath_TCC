import json
import unittest

from fastapi import HTTPException

from DTOs.Amostra import Amostra
from services.Infos.objetos import create_images
from services.risk.classification import classify_risk, parse_history


class RiskTests(unittest.TestCase):
    def classify(self, classes=(), ph=7, oxygen=6, levels=()):
        sample = Amostra(temperatura=22, ph=ph, condutividade_eletrica=100, oxigenio_dissolvido=oxygen)
        names = dict(enumerate(classes))
        images = create_images([([0, 0, 10, 10], 0.5, label) for label in names], names)
        history = parse_history(json.dumps([
            {"predictionId": index + 1, "coletaId": index + 1, "baseRiskLevel": level}
            for index, level in enumerate(levels)
        ]))
        return classify_risk(images, sample, history)

    def test_approved_three_levels(self):
        self.assertEqual(self.classify()["riskLevel"], 1)
        for name in ("Lixo", "drainage connection", "urbano"):
            with self.subTest(name=name):
                self.assertEqual(self.classify(classes=(name,))["riskLevel"], 2)
                self.assertEqual(self.classify(classes=(name,), oxygen=4.9)["riskLevel"], 3)
        self.assertEqual(self.classify(ph=9.1)["riskLevel"], 2)
        self.assertEqual(self.classify(oxygen=4.9)["riskLevel"], 2)

    def test_reference_boundaries(self):
        for ph in (6, 9):
            self.assertEqual(self.classify(ph=ph, oxygen=5)["riskLevel"], 1)
        self.assertEqual(self.classify(ph=5.99)["riskLevel"], 2)

    def test_unknown_and_repeated_visual_classes(self):
        self.assertEqual(self.classify(classes=("desconhecida", "turbidez"))["riskLevel"], 1)
        self.assertEqual(self.classify(classes=("Lixo", "Lixo", "urbano"))["riskLevel"], 2)
        self.assertEqual(self.classify(ph=5, oxygen=4)["riskLevel"], 2)

    def test_history_requires_three_occurrences(self):
        result = self.classify(levels=(2, 3, 1, 1, 1))
        self.assertEqual(result["riskLevel"], 1)
        result = self.classify(levels=(2, 3, 2, 1, 1))
        self.assertEqual(result["baseRiskLevel"], 1)
        self.assertEqual(result["riskLevel"], 2)
        self.assertEqual(result["history"]["adjustment"], 1)
        self.assertEqual(result["history"]["alertCollections"], 3)
        self.assertEqual(result["history"]["evaluatedCollections"], 5)
        self.assertEqual(len(result["history"]["samples"]), 5)
        self.assertTrue(any("Recorrência" in reason for reason in result["riskReasons"]))

    def test_history_never_exceeds_level_three(self):
        self.assertEqual(self.classify(classes=("Lixo",), levels=(2, 2, 2))["riskLevel"], 3)
        result = self.classify(classes=("Lixo",), oxygen=4, levels=(2, 2, 2))
        self.assertEqual(result["riskLevel"], 3)
        self.assertEqual(result["history"]["adjustment"], 0)

    def test_invalid_history(self):
        invalid = ["{}", "invalid", '[{"predictionId":1,"coletaId":1,"baseRiskLevel":4}]',
                   '[{"predictionId":1,"coletaId":1,"baseRiskLevel":true}]']
        invalid.append(json.dumps([{"predictionId":i + 1,"coletaId":1,"baseRiskLevel":2} for i in range(2)]))
        invalid.append(json.dumps([{"predictionId":i + 1,"coletaId":i + 1,"baseRiskLevel":2} for i in range(6)]))
        for data in invalid:
            with self.subTest(data=data), self.assertRaises(HTTPException) as error:
                parse_history(data)
            self.assertEqual(error.exception.status_code, 422)
