import type { ReportConfig } from "../types/reportConfig";

export function validateReportConfig(config: ReportConfig, today = new Date().toLocaleDateString("sv-SE", { timeZone: "America/Sao_Paulo" })): string | null {
  const valid = (value: string) => /^\d{4}-\d{2}-\d{2}$/.test(value) && Number.isFinite(Date.parse(value)) && new Date(value).toISOString().slice(0, 10) === value;
  if (!valid(config.startDate) || !valid(config.endDate) || config.startDate < "1900-01-01") return "Informe datas válidas para o período.";
  if (config.startDate > config.endDate) return "A data inicial deve ser anterior ou igual à data final.";
  if (config.endDate > today) return "O período não pode terminar no futuro.";
  if (Date.parse(config.endDate) - Date.parse(config.startDate) > 365 * 86400000) return "Selecione um período de até 366 dias.";
  if (!["resumo", "medio", "tecnico"].includes(config.detailLevel) || !["curto", "medio", "completo"].includes(config.documentLength)) return "Selecione detalhamento e extensão válidos.";
  if (!Object.values(config.includedSections).some(Boolean)) return "Selecione pelo menos uma seção.";
  return null;
}
export function reportConfigKey(config: ReportConfig) { return JSON.stringify(config); }
