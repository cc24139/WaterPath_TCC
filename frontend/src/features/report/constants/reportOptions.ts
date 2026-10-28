import type { DetailLevel, DocumentLength, ReportConfig } from "../types/reportConfig";

export function createDefaultReportConfig(now = new Date()): ReportConfig {
  const endDate = now.toLocaleDateString("sv-SE", { timeZone: "America/Sao_Paulo" });
  const start = new Date(`${endDate}T12:00:00Z`);
  start.setUTCDate(start.getUTCDate() - 29);
  return { startDate: start.toISOString().slice(0, 10), endDate, detailLevel: "medio", documentLength: "medio",
    includedSections: { executiveSummary: true, currentData: true, indicatorEvolution: true, heavyMetals: true, visualEvidence: true, detailedMethodology: false },
    additionalSettings: { highlightCriticalPoints: true, includeVisualAlerts: true } };
}
export const detailLevelOptions = [
  { value: "resumo", label: "Resumo" }, { value: "medio", label: "Médio" }, { value: "tecnico", label: "Técnico" },
] satisfies Array<{ value: DetailLevel; label: string }>;
export const documentLengthOptions = [
  { value: "curto", label: "Compacto" }, { value: "medio", label: "Padrão" }, { value: "completo", label: "Completo" },
] satisfies Array<{ value: DocumentLength; label: string }>;
export const includedSectionOptions = [
  { key: "executiveSummary", label: "Síntese do período" }, { key: "currentData", label: "Indicadores da última coleta" },
  { key: "indicatorEvolution", label: "Evolução das medições e riscos" }, { key: "heavyMetals", label: "Metais medidos e estimados" },
  { key: "visualEvidence", label: "Imagens e detecções da IA" }, { key: "detailedMethodology", label: "Metodologia detalhada" },
] satisfies Array<{ key: keyof ReportConfig["includedSections"]; label: string }>;
export const additionalSettingOptions = [
  { key: "highlightCriticalPoints", label: "Destacar motivos de atenção" }, { key: "includeVisualAlerts", label: "Usar cores na classificação de risco" },
] satisfies Array<{ key: keyof ReportConfig["additionalSettings"]; label: string }>;
