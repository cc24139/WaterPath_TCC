import assert from "node:assert/strict";
import test from "node:test";
import { buildRiverOverview } from "../src/features/water-body-monitoring/analysis/utils/riverOverview.ts";

const now = Date.parse("2026-10-26T12:00:00Z");
const measurement = (code, value, unit, extra = {}) => ({
  id: code, coletaId: 1, codigoMedicao: code, valor: value, unidade: unit, censurado: false, ...extra,
});
const collection = (id, date, ph = 7, overrides = {}) => ({
  id, corpoHidricoId: 1, dataHora: date, latitude: -22.9, longitude: -43.1, profundidadeMetros: 1,
  medicoes: [measurement(1, ph, "pH"), measurement(2, 6, "mg/L"), measurement(4, 200, "µS/cm"), measurement(17, 70, "pontos")],
  ...overrides,
});
const dates = ["2026-02-01", "2026-03-01", "2026-04-01", "2026-07-01", "2026-08-01", "2026-09-01"];
const samples = dates.map((date, index) => collection(index + 1, `${date}T12:00:00Z`, index < 3 ? 7 : 8));

test("compara metades cronológicas sem depender da ordem recebida", () => {
  const overview = buildRiverOverview([samples[5], samples[0], ...samples.slice(1, 5)], 365, now);
  assert.equal(overview.count, 6);
  assert.equal(overview.comparisonCount, 3);
  assert.deepEqual(overview.collections.map((item) => item.id), [6, 5, 4, 3, 2, 1]);
  assert.equal(overview.indicators[0].firstMedian, 7);
  assert.equal(overview.indicators[0].lastMedian, 8);
  assert.equal(overview.indicators[0].groupSize, 3);
  assert.equal(overview.latestIqa, 70);
});

test("não compara amostras escassas, próximas no tempo ou fora do período", () => {
  assert.equal(buildRiverOverview(samples, 90, now).comparisonCount, 0);
  const sameWeek = samples.map((item, index) => ({ ...item, dataHora: `2026-10-${String(index + 1).padStart(2, "0")}T12:00:00Z` }));
  assert.equal(buildRiverOverview(sameWeek, 365, now).comparisonCount, 0);
  assert.equal(buildRiverOverview([], 365, now).latestIqa, null);
});

test("suspende a comparação entre locais ou profundidades diferentes", () => {
  const moved = samples.map((item, index) => index === 5 ? { ...item, latitude: -22.89 } : item);
  const overview = buildRiverOverview(moved, 365, now);
  assert.equal(overview.mixedContext, true);
  assert.equal(overview.comparisonCount, 0);
  assert.equal(overview.count, 6);
});

test("exclui unidades incorretas e valores censurados das comparações", () => {
  const invalid = samples.map((item, index) => index === 5 ? { ...item, medicoes: [
    measurement(1, 8, "pH", { censurado: true }),
    measurement(2, 6, "ppm"),
    measurement(4, 200, "µS/cm"),
    measurement(17, 70, "pontos"),
  ] } : item);
  const overview = buildRiverOverview(invalid, 365, now);
  assert.equal(overview.indicators[0].count, 5);
  assert.equal(overview.indicators[0].firstMedian, null);
  assert.equal(overview.indicators[1].count, 5);
  assert.equal(overview.indicators[2].count, 6);
  assert.equal(overview.comparisonCount, 1);
});
