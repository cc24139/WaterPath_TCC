export type ForecastPeriod = "7" | "15" | "30" | "60" | "90";
export type DetailLevel = "resumo" | "medio" | "tecnico";
export type DocumentLength = "curto" | "medio" | "completo";

export interface ReportConfig {
  forecastPeriod: ForecastPeriod;
  detailLevel: DetailLevel;
  documentLength: DocumentLength;
  includedSections: {
    executiveSummary: boolean;
    currentData: boolean;
    indicatorEvolution: boolean;
    prediction: boolean;
    recommendations: boolean;
    detailedMethodology: boolean;
  };
  additionalSettings: {
    highlightCriticalPoints: boolean;
    includeVisualAlerts: boolean;
    includeCountermeasureTable: boolean;
    technicalLanguage: boolean;
  };
}
