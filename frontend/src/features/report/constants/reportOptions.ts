import type {
  DetailLevel,
  DocumentLength,
  ForecastPeriod,
  ReportConfig,
} from "../types/reportConfig";

export const defaultReportConfig: ReportConfig = {
  forecastPeriod: "30",
  detailLevel: "medio",
  documentLength: "medio",
  includedSections: {
    executiveSummary: true,
    currentData: true,
    indicatorEvolution: true,
    prediction: true,
    recommendations: true,
    detailedMethodology: false,
  },
  additionalSettings: {
    highlightCriticalPoints: true,
    includeVisualAlerts: true,
    includeCountermeasureTable: true,
    technicalLanguage: false,
  },
};

export const forecastPeriodOptions = [
  { value: "7", label: "7 Dias" },
  { value: "15", label: "15 Dias" },
  { value: "30", label: "30 Dias" },
  { value: "60", label: "60 Dias" },
  { value: "90", label: "90 Dias" },
] satisfies Array<{ value: ForecastPeriod; label: string }>;

export const detailLevelOptions = [
  { value: "resumo", label: "Resumo" },
  { value: "medio", label: "Médio" },
  { value: "tecnico", label: "Técnico" },
] satisfies Array<{ value: DetailLevel; label: string }>;

export const documentLengthOptions = [
  { value: "curto", label: "Curto (1-2 páginas)" },
  { value: "medio", label: "Médio (3-5 páginas)" },
  { value: "completo", label: "Completo (6+ páginas)" },
] satisfies Array<{ value: DocumentLength; label: string }>;

export const includedSectionOptions = [
  { key: "executiveSummary", label: "Resumo executivo" },
  { key: "currentData", label: "Dados atuais" },
  { key: "indicatorEvolution", label: "Evolução dos indicadores" },
  { key: "prediction", label: "Predição" },
  { key: "recommendations", label: "Recomendações" },
  { key: "detailedMethodology", label: "Metodologia detalhada" },
] satisfies Array<{
  key: keyof ReportConfig["includedSections"];
  label: string;
}>;

export const additionalSettingOptions = [
  { key: "highlightCriticalPoints", label: "Destacar pontos críticos" },
  { key: "includeVisualAlerts", label: "Incluir alertas visuais" },
  { key: "includeCountermeasureTable", label: "Incluir tabela de contramedidas" },
  { key: "technicalLanguage", label: "Linguagem técnica" },
] satisfies Array<{
  key: keyof ReportConfig["additionalSettings"];
  label: string;
}>;
