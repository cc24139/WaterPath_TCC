import assert from "node:assert/strict";
import test from "node:test";
import { parseWaterBodyId } from "../src/features/water-body-monitoring/utils/waterBodyId.ts";
import { buildRivers, readNumber, readTimestamp } from "../src/features/search/utils/riverData.ts";
import { measurementChart } from "../src/features/search/utils/measurementChart.ts";

const bodies = [{ id: 1, nome: "Rio de teste", localizacao: "Niterói/RJ", users: [{ id: 3 }] }];

test("route IDs match positive backend Int32 values without ambiguous formats", () => {
  for (const value of ["", "0", "-1", "01", "1.5", "1e2", "+1", " 1", "abc", "2147483648", "999999999999999999999"]) {
    assert.equal(parseWaterBodyId(value), null, value);
  }
  assert.equal(parseWaterBodyId("1"), 1);
  assert.equal(parseWaterBodyId("2147483647"), 2147483647);
});
const collection = (overrides = {}) => ({ id: 1, corpoHidrico: { id: 1 }, data: "2026-01-01T12:00:00", ph: 7, condutividadeEletrica: 210, condutividadeEletricaUnidade: "µS/cm", oxigenioDissolvido: 5, ...overrides });

test("empty responses stay empty, without demo values", () => {
  assert.deepEqual(buildRivers([], [], []), { rivers: [], warnings: [] });
  const { rivers } = buildRivers(bodies, [], []);
  assert.equal(rivers[0].iqa, null);
  assert.equal(rivers[0].status, "Sem classificação");
  assert.deepEqual(rivers[0].measurements, []);
});

test("missing, malformed and nonfinite numbers never become zero", () => {
  for (const value of [null, undefined, "", " ", false, "NaN", Infinity, "7 mg/L"]) assert.equal(readNumber(value), null);
  assert.equal(readNumber("0"), 0);
  assert.equal(readNumber(" 7,25 "), 7.25);
  assert.equal(readNumber("7.25"), 7.25);
});

test("normalizes string IDs, PascalCase fields and flat relations", () => {
  const { rivers, warnings } = buildRivers([{ Id: "1", Nome: "Rio", Localizacao: "RJ", Users: [{ Id: "3" }] }], [
    { Id: "4", CorpoHidricoId: "1", Data: "2026-01-01", Ph: "7,2", CondutividadeEletrica: "0", CondutividadeEletricaUnidade: "µS/cm", OxigenioDissolvido: "5.1" },
  ], [{ Id: "1", CorpoHidricoId: "1", IQA: "82" }]);
  assert.deepEqual(warnings, []);
  assert.equal(rivers[0].measurements[0].ph, 7.2);
  assert.equal(rivers[0].measurements[0].conductivity, 0);
  assert.deepEqual(rivers[0].userIds, ["3"]);
  assert.equal(rivers[0].iqa, 82);
});

test("dates are validated and no-offset calendar dates are stable", () => {
  for (const value of [null, "", "01/02/2026", "2026-02-30", "0001-01-01T00:00:00", "2026-13-01"]) assert.equal(readTimestamp(value), null);
  assert.equal(readTimestamp("2026-01-01"), Date.UTC(2026, 0, 1));
  assert.equal(readTimestamp("2026-01-01T12:00:00"), Date.UTC(2026, 0, 1, 12));
  assert.equal(readTimestamp("2026-01-01T12:00:00-03:00"), Date.UTC(2026, 0, 1, 15));
});

test("sorts by collection date, not response order or ID; keeps missing latest values missing", () => {
  const { rivers } = buildRivers(bodies, [collection({ id: 2, data: "2026-06-01", ph: null }), collection({ id: 100, data: "2026-01-01" })], []);
  assert.deepEqual(rivers[0].measurements.map((item) => item.id), ["100", "2"]);
  assert.equal(rivers[0].measurements.at(-1).ph, null);
});

test("orphan, duplicate and invalid-date records are not assigned to a river", () => {
  const { rivers, warnings } = buildRivers(bodies, [collection(), collection(), collection({ id: 2, corpoHidrico: null }), collection({ id: 3, data: "bad" }), collection({ id: 4, corpoHidrico: { id: 99 } })], []);
  assert.equal(rivers[0].measurements.length, 1);
  assert.equal(warnings.length, 1);
});

test("invalid metrics leave a gap, valid zero measurements are preserved", () => {
  const { rivers } = buildRivers(bodies, [collection({ ph: 15, condutividadeEletrica: -1, oxigenioDissolvido: 0 })], []);
  const point = rivers[0].measurements[0];
  assert.equal(point.ph, null);
  assert.equal(point.conductivity, null);
  assert.equal(point.dissolvedOxygen, 0);
});

test("IQA is not rescaled or guessed from an unordered series", () => {
  assert.equal(buildRivers(bodies, [], [{ id: 1, corpoHidrico: { id: 1 }, iqa: 0.82 }]).rivers[0].iqa, 0.82);
  assert.equal(buildRivers(bodies, [], [{ id: 1, corpoHidrico: { id: 1 }, iqa: 101 }]).rivers[0].iqa, null);
  const river = buildRivers(bodies, [], [{ id: 100, corpoHidrico: { id: 1 }, iqa: 80 }, { id: 1, corpoHidrico: { id: 1 }, iqa: 40 }]).rivers[0];
  assert.equal(river.iqa, null);
  assert.equal(river.status, "Sem classificação");
  assert.deepEqual(river.iqaValues, [80, 40]);
});

test("invalid nonempty body payload fails instead of masquerading as an empty database", () => {
  assert.throws(() => buildRivers([{}], [], []), /formato inválido/);
});

test("chart uses elapsed time for unevenly spaced samples", () => {
  const measurements = buildRivers(bodies, [collection({ id: 1, data: "2026-01-01" }), collection({ id: 2, data: "2026-01-02" }), collection({ id: 3, data: "2026-01-11" })], []).rivers[0].measurements;
  const chart = measurementChart(measurements, "ph");
  assert.equal(chart.maximum, 14);
  assert.deepEqual(chart.points.map((point) => point.x), [45, 69, 285]);
  assert.equal(chart.segments.length, 2);
});

test("chart preserves missing-value gaps and does not draw a trend for one sample", () => {
  const measurements = buildRivers(bodies, [collection({ id: 1, data: "2026-01-01" }), collection({ id: 2, data: "2026-01-02", ph: null }), collection({ id: 3, data: "2026-01-03" })], []).rivers[0].measurements;
  assert.equal(measurementChart(measurements, "ph").segments.length, 0);
  const single = measurementChart(measurements.slice(0, 1), "ph");
  assert.equal(single.points[0].x, 165);
  assert.equal(single.segments.length, 0);
});

test("chart handles equal timestamps and constant zero values without dividing by zero", () => {
  const measurements = buildRivers(bodies, [collection({ id: 1, condutividadeEletrica: 0 }), collection({ id: 2, condutividadeEletrica: 0 })], []).rivers[0].measurements;
  const chart = measurementChart(measurements, "conductivity");
  assert.equal(chart.segments.length, 0);
  assert.ok(chart.maximum > 0);
  assert.ok(chart.points.every((point) => Number.isFinite(point.x) && Number.isFinite(point.y)));
});

test("current collection contract reads conductivity code 4, never turbidity code 3", () => {
  for (const code of [4, "4", "CondutividadeEletrica"]) {
    const { rivers } = buildRivers(bodies, [{ id: 10, corpoHidricoId: 1, dataHora: "2026-10-01T12:00:00Z", medicoes: [
      { codigoMedicao: 3, valor: 20, unidade: "NTU", censurado: false },
      { codigoMedicao: code, valor: 312, unidade: "µS/cm", censurado: false },
      { codigoMedicao: 1, valor: 7, unidade: "pH", censurado: false },
      { codigoMedicao: 2, valor: 6, unidade: "mg/L", censurado: false },
    ] }], []);
    assert.equal(rivers[0].measurements[0].conductivity, 312);
    assert.equal(rivers[0].measurements[0].ph, 7);
    assert.equal(rivers[0].measurements[0].dissolvedOxygen, 6);
  }
});

test("missing, censored, duplicate or incompatible conductivity is not plotted as an exact value", () => {
  const ec = { codigoMedicao: 4, valor: 100, unidade: "µS/cm", censurado: false };
  for (const medicoes of [[], [{ codigoMedicao: 3, valor: 10, unidade: "NTU" }], [{ ...ec, censurado: true, limite: 100 }], [{ ...ec, unidade: "mS/cm" }], [ec, ec]]) {
    const { rivers } = buildRivers(bodies, [collection({ medicoes })], []);
    assert.equal(rivers[0].measurements[0].conductivity, null);
  }
  assert.equal(buildRivers(bodies, [collection({ condutividadeEletricaUnidade: undefined })], []).rivers[0].measurements[0].conductivity, null);
  for (const unidade of ["µS/cm", "μS/cm", "uS/cm"]) {
    assert.equal(buildRivers(bodies, [collection({ medicoes: [{ ...ec, valor: 0, unidade }] })], []).rivers[0].measurements[0].conductivity, 0);
  }
});
