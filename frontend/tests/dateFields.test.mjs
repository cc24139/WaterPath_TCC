import assert from "node:assert/strict";
import test from "node:test";
import { calendarISO, calendarToday, toCalendarDate, displayDate, parseDisplayDate, dateInputText, rangeDays, rangeError, periodPresets, normalizeTime, timeInputText, dateError } from "../src/components/ui/date/dateUtils.ts";

test("datas brasileiras exigem dia real e preservam ano bissexto", () => {
  assert.equal(parseDisplayDate("29/02/2024"), "2024-02-29");
  assert.equal(parseDisplayDate("28/10/2026"), "2026-10-28");
  for (const text of ["29/02/2025", "31/04/2026", "00/10/2026", "01/13/2026", "01/01/1899", "1/1/2026", "", "abc"])
    assert.equal(parseDisplayDate(text), undefined);
});
test("digitação e colagem mantêm formato brasileiro sem inventar datas", () => {
  assert.equal(dateInputText("28102026"), "28/10/2026");
  assert.equal(dateInputText("2026-10-28"), "28/10/2026");
  assert.equal(dateInputText("28/1"), "28/1");
  assert.equal(dateInputText(""), "");
  assert.equal(displayDate("2026-02-30"), "");
  assert.equal(dateError("2026-10-29", "1900-01-01", "2026-10-28"), "Escolha uma data até 28/10/2026.");
});
test("data de calendário não muda de dia em fusos diferentes", () => {
  const previous = process.env.TZ;
  try {
    for (const zone of ["UTC", "America/Sao_Paulo", "Pacific/Kiritimati", "America/New_York"]) {
      process.env.TZ = zone;
      assert.equal(calendarISO(toCalendarDate("2026-10-28")), "2026-10-28");
      assert.equal(rangeDays("2026-03-07", "2026-03-09"), 3);
      assert.equal(calendarToday("America/Sao_Paulo", new Date("2026-10-29T01:00:00Z")), "2026-10-28");
    }
  } finally { if (previous === undefined) delete process.env.TZ; else process.env.TZ = previous; }
});
test("atalhos incluem hoje, respeitam virada do ano e fevereiro bissexto", () => {
  const presets = periodPresets("2026-01-03");
  assert.deepEqual(presets[0], { label: "Últimos 7 dias", from: "2025-12-28", to: "2026-01-03" });
  assert.equal(rangeDays(presets[1].from, presets[1].to), 30);
  assert.deepEqual(presets[3], { label: "Mês anterior", from: "2025-12-01", to: "2025-12-31" });
  assert.equal(periodPresets("2024-03-01")[3].to, "2024-02-29");
});
test("intervalo permite um dia e bloqueia ordem, futuro e excesso de dias", () => {
  const min = "1900-01-01", max = "2026-10-28";
  assert.equal(rangeError(max, max, min, max), null);
  assert.equal(rangeError("2024-01-01", "2024-12-31", min, max), null);
  for (const [from, to] of [["2026-10-28", "2026-10-01"], ["2026-10-01", "2026-10-29"], ["2024-01-01", "2025-01-01"], ["", max]])
    assert.ok(rangeError(from, to, min, max));
});
test("horário usa 24 horas e aceita teclado numérico sem precisar digitar dois pontos", () => {
  assert.equal(timeInputText("0930"), "09:30");
  assert.equal(timeInputText("9:30"), "09:30");
  assert.equal(normalizeTime("00:00"), "00:00");
  assert.equal(normalizeTime("23:59"), "23:59");
  for (const text of ["24:00", "12:60", "-1:30", "9:", "abc", ""]) assert.equal(normalizeTime(text), undefined);
});
