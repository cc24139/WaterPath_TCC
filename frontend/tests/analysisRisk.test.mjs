import assert from "node:assert/strict";
import test from "node:test";
import { isRiskLevel, readAnalysisSummaries } from "../src/features/analysis-risk/utils/analysisRisk.ts";

const row = (overrides = {}) => ({ coletaId: 1, corpoHidricoId: 2, corpoHidricoNome: "Rio", localizacao: "Local",
  dataHora: "2026-10-01T12:00:00Z", responsavelNome: "Pessoa", predicaoId: 3,
  analisadaEm: "2026-10-02T12:00:00Z", risco: { level: 2, reasons: ["Motivo retornado pela IA"] }, ...overrides });
const page = (rows) => ({ total: rows.length, pagina: 1, tamanhoPagina: 10, itens: rows });

test("aceita somente níveis 1, 2 e 3, sem inferir nível a partir de strings ou percentuais", () => {
  for (const value of [1, 2, 3]) assert.equal(isRiskLevel(value), true);
  for (const value of [0, 4, -1, 50, "1", null, undefined, NaN]) assert.equal(isRiskLevel(value), false);
});
test("preserva motivos e separa coleta pendente de resultado existente inválido", () => {
  const data = readAnalysisSummaries(page([row(), row({ coletaId: 2, predicaoId: null, analisadaEm: null, risco: null }), row({ coletaId: 3, risco: { level: 0, reasons: [] } })]));
  assert.equal(data.itens[0].risco.level, 2);
  assert.deepEqual(data.itens[0].risco.reasons, ["Motivo retornado pela IA"]);
  assert.equal(data.itens[1].predicaoId, null);
  assert.equal(data.itens[2].risco, null);
  assert.equal(data.itens[2].predicaoId, 3);
});
test("não apresenta risco sem predição ou com motivos malformados e não altera a resposta original", () => {
  for (const overrides of [{ predicaoId: null }, { analisadaEm: null }, { risco: { level: 1, reasons: [42] } }]) {
    const input = page([row(overrides)]);
    const snapshot = structuredClone(input);
    assert.equal(readAnalysisSummaries(input).itens[0].risco, null);
    assert.deepEqual(input, snapshot);
  }
});
test("rejeita resposta estruturalmente inválida e IDs duplicados", () => {
  for (const input of [null, {}, page([row(), row()]), page([row({ dataHora: "invalid" })]), page([row({ coletaId: -1 })]), { ...page([]), pagina: 0 }, { ...page([]), total: -1 }]) {
    assert.throws(() => readAnalysisSummaries(input));
  }
  assert.deepEqual(readAnalysisSummaries(page([])).itens, []);
});
