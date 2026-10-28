import assert from "node:assert/strict";
import test from "node:test";
import { createDefaultReportConfig } from "../src/features/report/constants/reportOptions.ts";
import { validateReportConfig, reportConfigKey } from "../src/features/report/utils/reportConfig.ts";
import { readReportPdf } from "../src/features/report/utils/reportResponse.ts";

test("período padrão contém 30 datas e usa o dia de Brasília", () => {
  const config = createDefaultReportConfig(new Date("2026-10-29T01:00:00Z"));
  assert.equal(config.endDate, "2026-10-28"); assert.equal(config.startDate, "2026-09-29");
  assert.equal(validateReportConfig(config, "2026-10-28"), null);
});
test("valida datas reais, ordem, futuro, limite de período e seções", () => {
  const config = createDefaultReportConfig(new Date("2026-10-28T12:00:00Z"));
  for (const overrides of [{ startDate: "2026-02-30" }, { startDate: "2026-11-01" }, { endDate: "2026-10-29" }, { startDate: "2025-01-01" }, { includedSections: Object.fromEntries(Object.keys(config.includedSections).map(key => [key, false])) }]) {
    assert.ok(validateReportConfig({ ...config, ...overrides }, "2026-10-28"));
  }
  assert.equal(validateReportConfig({ ...config, startDate: config.endDate }, "2026-10-28"), null);
});
test("a configuração identifica prévias desatualizadas incluindo mudanças de seção", () => {
  const config = createDefaultReportConfig();
  assert.equal(reportConfigKey(config), reportConfigKey(structuredClone(config)));
  assert.notEqual(reportConfigKey(config), reportConfigKey({ ...config, includedSections: { ...config.includedSections, heavyMetals: false } }));
});
test("prévia recebe PDF validado e rejeita HTML ou arquivo vazio", async () => {
  const pdf = "%PDF-1.7\nconteudo-teste";
  const blob = await readReportPdf(new Response(pdf, { headers: { "content-type": "application/pdf" } }));
  assert.equal(await blob.text(), pdf);
  await assert.rejects(readReportPdf(new Response("<html>erro</html>")), /documento inválido/);
  await assert.rejects(readReportPdf(new Response("", { headers: { "content-type": "application/pdf" } })), /vazio ou inválido/);
});
test("falhas HTTP preservam orientações de sessão, permissão, versão e ausência de dados", async () => {
  for (const [status, expected] of [[401, /sessão expirou/], [403, /permissão/], [404, /não está disponível/]])
    await assert.rejects(readReportPdf(new Response(null, { status })), expected);
  await assert.rejects(readReportPdf(new Response(JSON.stringify({ detail: "Nenhuma coleta no período." }), { status: 422 })), /Nenhuma coleta/);
  await assert.rejects(readReportPdf(new Response("<html>SQL</html>", { status: 500 })), error => !error.message.includes("SQL") && !error.message.includes("html"));
});
