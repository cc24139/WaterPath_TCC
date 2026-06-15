export interface AddAnalysisFormValues {
  waterBody: string;
  analysisDate: string;
  collectionPoint: string;
  responsible: string;
  analysisType: string;
  ph: string;
  temperature: string;
  turbidity: string;
  conductivity: string;
  tds: string;
  heavyMetals: string;
  visualCondition: string;
  observations: string;
  manualDiagnosis: string;
}

export interface AddAnalysisFormState extends AddAnalysisFormValues {
  imageName: string;
  imagePreviewUrl: string;
}

export type AddAnalysisFieldName = keyof AddAnalysisFormValues;

export interface SelectOption {
  label: string;
  value: string;
}

export interface AnalysisFieldConfig {
  name: AddAnalysisFieldName;
  label: string;
  placeholder: string;
  helper?: string;
  type?: "text" | "date";
  inputMode?: "decimal" | "numeric" | "text";
}
