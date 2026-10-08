import copy
import json
import unittest

from fastapi import HTTPException

from DTOs.Amostra import Amostra
from DTOs.CollectionContext import parse_collection_context
from services.risk.classification import parse_history


class CollectionContextTests(unittest.TestCase):
    def setUp(self):
        self.sample = Amostra(temperatura=22, ph=7, condutividade_eletrica=100,
                              oxigenio_dissolvido=6, data="2026-10-07T12:00:00Z")
        self.context = {
            "waterBodyId": 1, "currentCollectionId": 10, "currentCollectedAt": self.sample.data,
            "history": [{"waterBodyId": 1, "collectionId": i,
                         "collectedAt": f"2026-10-0{i}T12:00:00Z",
                         "measurements": [{"code": "Ph", "value": None, "unit": "pH",
                                           "censored": True, "limit": 6}], "metals": []}
                        for i in range(1, 7)],
        }

    def parse(self, context=None, history="[]"):
        return parse_collection_context(json.dumps(self.context if context is None else context),
                                        self.sample, parse_history(history))

    def test_preserves_all_observations_and_empty_history(self):
        self.assertEqual(self.parse(), self.context)
        self.context["history"] = []
        self.assertEqual(self.parse()["history"], [])
        self.assertIsNone(parse_collection_context(None, self.sample, []))

    def test_rejects_other_lake_current_duplicate_future_and_unsorted_history(self):
        for kind in ("other", "current", "duplicate", "future", "unsorted", "date", "unknown", "nonfinite"):
            context = copy.deepcopy(self.context)
            if kind == "other": context["history"][0]["waterBodyId"] = 2
            if kind == "current": context["history"][0]["collectionId"] = 10
            if kind == "duplicate": context["history"][1]["collectionId"] = 1
            if kind == "future": context["history"][0]["collectedAt"] = self.sample.data
            if kind == "unsorted": context["history"].reverse()
            if kind == "date": context["currentCollectedAt"] = "2026-10-07T12:00:00"
            if kind == "unknown": context["ignored"] = True
            if kind == "nonfinite": context["history"][0]["measurements"][0]["value"] = float("nan")
            with self.subTest(kind=kind), self.assertRaises(HTTPException) as error:
                self.parse(context)
            self.assertEqual(error.exception.status_code, 422)

    def test_risk_window_must_belong_to_last_five_collections(self):
        self.parse(history='[{"predictionId":1,"coletaId":2,"baseRiskLevel":2}]')
        with self.assertRaises(HTTPException):
            self.parse(history='[{"predictionId":1,"coletaId":1,"baseRiskLevel":2}]')

    def test_current_date_must_match_sample(self):
        self.sample.data = "2026-10-06T12:00:00Z"
        with self.assertRaises(HTTPException):
            self.parse()


if __name__ == "__main__":
    unittest.main()
