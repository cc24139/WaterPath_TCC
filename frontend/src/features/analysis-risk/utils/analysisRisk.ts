export type RiskLevel = 1 | 2 | 3;
export const riskLabels: Record<RiskLevel, string> = { 1: "Baixo", 2: "Moderado", 3: "Alto" };
export function isRiskLevel(value: unknown): value is RiskLevel { return value === 1 || value === 2 || value === 3; }

export interface AnalysisSummary {
  coletaId: number;
  corpoHidricoId: number;
  corpoHidricoNome: string;
  localizacao: string;
  dataHora: string;
  responsavelNome: string | null;
  predicaoId: number | null;
  analisadaEm: string | null;
  risco: { level: RiskLevel; reasons: string[] } | null;
}
export interface AnalysisSummaryPage { total: number; pagina: number; tamanhoPagina: number; itens: AnalysisSummary[] }

export function readAnalysisSummaries(value: unknown): AnalysisSummaryPage {
  const page = value as AnalysisSummaryPage | null;
  const positive = (id: number) => Number.isSafeInteger(id) && id > 0;
  const date = (value: unknown) => typeof value === "string" && Number.isFinite(Date.parse(value));
  if (!page || !Number.isSafeInteger(page.total) || page.total < 0 || !positive(page.pagina)
    || !positive(page.tamanhoPagina) || page.tamanhoPagina > 100 || !Array.isArray(page.itens)
    || page.itens.length > page.tamanhoPagina || page.itens.length > page.total) throw new Error("Resposta de análises inválida.");
  const ids = new Set<number>();
  for (const item of page.itens) {
    if (!item || !positive(item.coletaId) || ids.has(item.coletaId) || !positive(item.corpoHidricoId)
      || typeof item.corpoHidricoNome !== "string" || typeof item.localizacao !== "string" || !date(item.dataHora)
      || (item.responsavelNome !== null && typeof item.responsavelNome !== "string")
      || (item.predicaoId !== null && !positive(item.predicaoId))
      || (item.analisadaEm !== null && !date(item.analisadaEm))) throw new Error("Resumo de análise inválido.");
    ids.add(item.coletaId);
  }
  return { ...page, itens: page.itens.map(item => ({ ...item,
    // Resultado inválido nunca é convertido em risco baixo nem derruba as demais linhas.
    risco: item.predicaoId && item.analisadaEm && item.risco && isRiskLevel(item.risco.level)
      && Array.isArray(item.risco.reasons) && item.risco.reasons.every(reason => typeof reason === "string") ? item.risco : null,
  })) };
}
