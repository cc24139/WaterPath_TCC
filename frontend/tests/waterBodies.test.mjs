import assert from "node:assert/strict";
import test from "node:test";
import { buildRivers, readNumber, readTimestamp } from "../src/features/search/utils/riverData.ts";
import { measurementChart } from "../src/features/search/utils/measurementChart.ts";

const bodies = [{ id: 1, nome: "Rio de teste", localizacao: "Niterói/RJ", users: [{ id: 3 }] }];
const collection = (overrides = {}) => ({ id: 1, corpoHidrico: { id: 1 }, data: "2026-01-01T12:00:00", ph: 7, turbidez: 2, oxigenioDissolvido: 5, ...overrides });

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
    { Id: "4", CorpoHidricoId: "1", Data: "2026-01-01", Ph: "7,2", Turbidez: "0", OxigenioDissolvido: "5.1" },
  ], [{ Id: "1", CorpoHidricoId: "1", IQA: "82" }]);
  assert.deepEqual(warnings, []);
  assert.equal(rivers[0].measurements[0].ph, 7.2);
  assert.equal(rivers[0].measurements[0].turbidity, 0);
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
  const { rivers } = buildRivers(bodies, [collection({ ph: 15, turbidez: -1, oxigenioDissolvido: 0 })], []);
  const point = rivers[0].measurements[0];
  assert.equal(point.ph, null);
  assert.equal(point.turbidity, null);
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
  const measurements = buildRivers(bodies, [collection({ id: 1, turbidez: 0 }), collection({ id: 2, turbidez: 0 })], []).rivers[0].measurements;
  const chart = measurementChart(measurements, "turbidity");
  assert.equal(chart.segments.length, 0);
  assert.ok(chart.maximum > 0);
  assert.ok(chart.points.every((point) => Number.isFinite(point.x) && Number.isFinite(point.y)));
});
