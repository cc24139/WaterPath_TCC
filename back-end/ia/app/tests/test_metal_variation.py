import json
import unittest

from pydantic import ValidationError

from DTOs.Amostra import Amostra
from services.risk.classification import classify_risk


class MetalVariationTests(unittest.TestCase):
    def sample(self, current=None, previous=None, date="2026-10-07T12:00:00Z", reference_date="2026-10-06T12:00:00Z"):
        values = dict(temperatura=22, ph=7, condutividade_eletrica=100, oxigenio_dissolvido=6, data=date)
        if current is not None:
            values["metais_pesados"] = current
        if previous is not None:
            values["referencia_metais_pesados"] = {"data": reference_date, "metais_pesados": previous}
        return Amostra.model_validate(values)

    def analyze(self, **kwargs):
        return classify_risk([], self.sample(**kwargs), [])["metalVariation"]

    def metal(self, name="Pb", value=10, unit="µg/L"):
        return dict(name=name, value=value, unit=unit)

    def test_increase_reduction_and_stability_per_metal(self):
        result = self.analyze(current=[self.metal("Pb", 10), self.metal("Cd", 1), self.metal("Fe", 0.1, "mg/L")],
                              previous=[self.metal("Pb", 5), self.metal("Cd", 2), self.metal("Fe", 0.1, "mg/L")])
        self.assertEqual(result["status"], "completa")
        states = {item["name"]: (item["variation"], item["delta"]) for item in result["metals"]}
        self.assertEqual(states, {"Pb": ("aumento", 5), "Cd": ("reducao", -1), "Fe": ("estabilidade", 0)})

    def test_compatible_units_are_converted_without_false_increase(self):
        for unit in ("µg/L", "μg/L", "ug/L"):
            with self.subTest(unit=unit):
                result = self.analyze(current=[self.metal(value=0.1, unit="mg/L")],
                                      previous=[self.metal(value=100, unit=unit)])
                self.assertEqual(result["metals"][0]["variation"], "estabilidade")
                self.assertEqual(result["metals"][0]["delta"], 0)
        result = self.analyze(current=[self.metal(value=150)], previous=[self.metal(value=0.1, unit="mg/L")])
        self.assertEqual(result["metals"][0]["delta"], 50)
        self.assertEqual(result["metals"][0]["unit"], "µg/L")

    def test_missing_metals_are_explicitly_indeterminate(self):
        result = self.analyze()
        self.assertEqual(result["status"], "indeterminada")
        self.assertEqual(result["metals"], [])
        self.assertIn("Não é possível determinar", result["message"])
        for current, previous in (([self.metal()], None), (None, [self.metal()]),
                                  ([self.metal(value=None)], [self.metal()])):
            with self.subTest(current=current, previous=previous):
                item = self.analyze(current=current, previous=previous)["metals"][0]
                self.assertEqual(item["variation"], "indeterminada")
                self.assertEqual(item["reason"], "dados_ausentes")
                self.assertIsNone(item["delta"])

    def test_partial_result_does_not_hide_unmatched_metals(self):
        result = self.analyze(current=[self.metal("Pb", 10), self.metal("Cd", 2)],
                              previous=[self.metal("Pb", 5), self.metal("Fe", 0.2, "mg/L")])
        self.assertEqual(result["status"], "parcial")
        self.assertEqual({m["name"]: m["variation"] for m in result["metals"]},
                         {"Pb": "aumento", "Cd": "indeterminada", "Fe": "indeterminada"})

    def test_incompatible_or_unknown_units_are_not_compared(self):
        for current_unit, previous_unit in (("mg/kg", "µg/L"), ("ppm", "ppm")):
            item = self.analyze(current=[self.metal(unit=current_unit)], previous=[self.metal(unit=previous_unit)])["metals"][0]
            self.assertEqual(item["variation"], "indeterminada")
            self.assertEqual(item["reason"], "unidades_incompativeis")
            self.assertIsNone(item["delta"])

    def test_missing_units_are_not_assumed(self):
        for unit in (None, " "):
            item = self.analyze(current=[self.metal(unit=unit)], previous=[self.metal()])["metals"][0]
            self.assertEqual(item["reason"], "unidades_ausentes")
            self.assertEqual(item["variation"], "indeterminada")

    def test_temporal_reference_must_be_present_valid_and_earlier(self):
        for date in (None, "invalid", "2026-10-06", "2026-10-06T12:00:00"):
            item = self.analyze(current=[self.metal()], previous=[self.metal(value=5)], reference_date=date)["metals"][0]
            self.assertEqual(item["reason"], "referencia_temporal_ausente_ou_invalida")
        for date in ("2026-10-07T12:00:00Z", "2026-10-08T12:00:00Z"):
            item = self.analyze(current=[self.metal()], previous=[self.metal(value=5)], reference_date=date)["metals"][0]
            self.assertEqual(item["reason"], "referencia_nao_anterior")
        item = self.analyze(current=[self.metal()], previous=[self.metal(value=5)], date=None)["metals"][0]
        self.assertEqual(item["variation"], "indeterminada")
        result = self.analyze(current=[self.metal()], previous=[self.metal(value=5)],
                              reference_date="2026-10-07T08:00:00-03:00")
        self.assertEqual(result["metals"][0]["variation"], "aumento")

    def test_nonrepresentable_delta_remains_indeterminate(self):
        for current, previous in ((self.metal(value=0, unit="µg/L"), self.metal(value=1e308, unit="mg/L")),
                                  (self.metal(value=0, unit="mg/L"), self.metal(value=1e-323, unit="µg/L"))):
            result = self.analyze(current=[current], previous=[previous])
            item = result["metals"][0]
            self.assertEqual(item["variation"], "indeterminada")
            self.assertIsNone(item["delta"])
            self.assertEqual(item["reason"], "variacao_nao_representavel")
            json.dumps(result, allow_nan=False)

    def test_zero_is_a_valid_measurement(self):
        item = self.analyze(current=[self.metal(value=0)], previous=[self.metal(value=1)])["metals"][0]
        self.assertEqual(item["variation"], "reducao")
        self.assertEqual(item["delta"], -1)

    def test_variation_does_not_create_a_limit_or_change_existing_risk(self):
        sample = self.sample(current=[self.metal(value=10000)], previous=[self.metal(value=0)])
        baseline = classify_risk([], self.sample(), [])
        result = classify_risk([], sample, [])
        for key in ("riskLevel", "baseRiskLevel", "riskLabel", "riskReasons", "riskRuleVersion", "riskInputs", "history"):
            self.assertEqual(result[key], baseline[key])
        self.assertEqual(result["metalVariation"]["metals"][0]["variation"], "aumento")
        self.assertEqual(sample.decode(), self.sample().decode())
        self.assertNotIn("limit", result["metalVariation"]["metals"][0])
        json.dumps(result, allow_nan=False)

    def test_invalid_observations_are_rejected(self):
        invalid = ([self.metal(value=-1)], [self.metal(value=float("inf"))], [self.metal(value=True)],
                   [self.metal(value="1")], [self.metal(), self.metal()], [self.metal(name="unknown")],
                   [dict(self.metal(), unexpected=1)], [self.metal(unit="")])
        for items in invalid:
            for field in ("current", "previous"):
                with self.subTest(items=items, field=field), self.assertRaises(ValidationError):
                    self.sample(**{field: items})
