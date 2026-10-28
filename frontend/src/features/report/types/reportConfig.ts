export type DetailLevel = "resumo" | "medio" | "tecnico";
export type DocumentLength = "curto" | "medio" | "completo";
export interface ReportConfig {
  startDate: string;
  endDate: string;
  detailLevel: DetailLevel;
  documentLength: DocumentLength;
  includedSections: {
    executiveSummary: boolean;
    currentData: boolean;
    indicatorEvolution: boolean;
    heavyMetals: boolean;
    visualEvidence: boolean;
    detailedMethodology: boolean;
  };
  additionalSettings: { highlightCriticalPoints: boolean; includeVisualAlerts: boolean };
}
