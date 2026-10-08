export type IndicatorName =
  | "temperatura" | "ph" | "condutividade_eletrica" | "oxigenio_dissolvido"
  | "solidos_suspensos_totais" | "carbono_organico_total" | "fosforo_total";
export type MetalSymbol = "Fe" | "Mn" | "Cr" | "Ni" | "Cu" | "Zn" | "Cd" | "Pb";
export type AddAnalysisFieldName = "waterBody" | "analysisDate" | IndicatorName;
export type AddAnalysisFormState = Record<AddAnalysisFieldName, string> & {
  metals: Partial<Record<MetalSymbol, string>>;
};
export type AddAnalysisErrors = Partial<Record<AddAnalysisFieldName | MetalSymbol | "image", string>>;
export interface SelectOption { label: string; value: string }
export interface AnalysisFieldConfig {
  name: IndicatorName;
  label: string;
  unit: string;
  measurementCode: string;
  placeholder: string;
  required: boolean;
}
