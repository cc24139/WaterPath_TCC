import assert from "node:assert/strict";
import test from "node:test";
import { readDashboardCollections, dashboardMetrics, getMeasurement, measurementValue, formatMetric, filterCollections, buildMeasurementsCsv } from "../src/features/water-body-monitoring/dashboard/utils/dashboardData.ts";

const measurement = (overrides = {}) => ({ id: 1, coletaId: 1, codigoMedicao: 1, valor: 7.02, unidade: "", censurado: false, limite: null, ...overrides });
const collection = (overrides = {}) => ({ id: 1, corpoHidricoId: 12, dataHora: "2026-10-05T08:30:00-03:00", medicoes: [measurement()], ...overrides });

test("backend collections are sorted by timestamp and original response is preserved", () => {
  const input = [collection({ id: 30, dataHora: "2026-10-01T11:00:00Z" }), collection({ id: 2 })];
  assert.deepEqual(readDashboardCollections(input, 12).map((item) => item.id), [2, 30]);
  assert.equal(input[0].id, 30);
});

test("wrong body, malformed lists, invalid dates and duplicate IDs fail explicitly", () => {
  for (const input of [{}, [collection({ corpoHidricoId: 13 })], [collection({ dataHora: "invalid" })], [collection(), collection()], [collection({ medicoes: [null] })]]) {
    assert.throws(() => readDashboardCollections(input, 12));
  }
  assert.deepEqual(readDashboardCollections([], 12), []);
});

test("enum codes support numeric and string serialization, preserving temperature zero", () => {
  for (const code of [0, "0", "Temperatura"]) {
    const result = getMeasurement(collection({ medicoes: [measurement({ codigoMedicao: code, valor: 0, unidade: "°C" })] }), dashboardMetrics[3]);
    assert.equal(measurementValue(result), 0);
    assert.equal(formatMetric(result), "0 °C");
  }
  assert.equal(formatMetric(undefined), "—");
});

test("censored values show the detection limit and are excluded from numerical charts", () => {
  const value = measurement({ valor: null, censurado: true, limite: 0.42, unidade: "µS/cm" });
  assert.equal(formatMetric(value), "< 0,42 µS/cm");
  assert.equal(measurementValue(value), null);
  const omittedNulls = measurement({ censurado: true, limite: 0.42 });
  delete omittedNulls.valor;
  assert.equal(readDashboardCollections([collection({ medicoes: [omittedNulls] })], 12).length, 1);
  assert.equal(formatMetric(omittedNulls), "< 0,42");
});

test("period filtering respects offset timestamps, boundaries and future collections", () => {
  const now = Date.parse("2026-10-05T11:30:00Z");
  const items = [collection(), collection({ id: 2, dataHora: "2026-09-28T11:30:00Z" }), collection({ id: 3, dataHora: "2026-09-28T11:29:59Z" }), collection({ id: 4, dataHora: "2026-10-05T11:30:01Z" })];
  assert.deepEqual(filterCollections(items, 7, now).map((item) => item.id), [1, 2]);
});

test("CSV includes units, censoring and escaped cells without formula execution", () => {
  const csv = buildMeasurementsCsv([collection({ medicoes: [measurement({ unidade: '=bad;"unit"' })] })]);
  assert.ok(csv.startsWith("\uFEFF"));
  assert.ok(csv.includes('7,02 =bad;""unit""'));
  assert.ok(csv.includes('"IQA"') === false);
});

test("conductivity cards, charts and CSV use the backend conductivity code and unit", () => {
  const metric = dashboardMetrics.find((item) => item.key === "conductivity");
  assert.ok(metric);
  for (const code of [4, "4", "CondutividadeEletrica"]) {
    const item = collection({ medicoes: [measurement({ codigoMedicao: 3, valor: 20, unidade: "NTU" }), measurement({ codigoMedicao: code, valor: 312, unidade: "µS/cm" })] });
    assert.equal(formatMetric(getMeasurement(item, metric)), "312 µS/cm");
    const csv = buildMeasurementsCsv([item]);
    assert.ok(csv.includes('"Condutividade elétrica"'));
    assert.ok(csv.includes('"312 µS/cm"'));
    assert.ok(!csv.includes("NTU"));
  }
  assert.equal(getMeasurement(collection({ medicoes: [measurement({ codigoMedicao: 3, unidade: "NTU" })] }), metric), undefined);
});
