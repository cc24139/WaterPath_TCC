import type {
  AddAnalysisFormState,
  AnalysisFieldConfig,
  SelectOption,
} from "@/features/add-analysis/types/addAnalysis";

export const defaultImagePreviewUrl = "/images/water-path-hero-river.png";

export const initialAddAnalysisForm: AddAnalysisFormState = {
  waterBody: "",
  analysisDate: "2026-04-12",
  collectionPoint: "",
  responsible: "",
  analysisType: "",
  ph: "6,4",
  temperature: "24,3",
  turbidity: "48,5",
  conductivity: "312",
  tds: "156",
  heavyMetals: "0,045",
  visualCondition: "",
  observations: "",
  manualDiagnosis: "",
  imageName: "",
  imagePreviewUrl: defaultImagePreviewUrl,
};

export const responsibleOptions: SelectOption[] = [
  { label: "Equipe Water Path", value: "Equipe Water Path" },
  { label: "Ana Souza", value: "Ana Souza" },
  { label: "Carlos Lima", value: "Carlos Lima" },
];

export const visualConditionOptions: SelectOption[] = [
  { label: "Água clara", value: "Água clara" },
  { label: "Levemente turva", value: "Levemente turva" },
  { label: "Turva com resíduos", value: "Turva com resíduos" },
  { label: "Presença de espuma", value: "Presença de espuma" },
];

export const indicatorFields: AnalysisFieldConfig[] = [
  {
    name: "ph",
    label: "pH",
    placeholder: "6,4",
    helper: "Ideal entre 6,0 e 8,5",
    inputMode: "decimal",
  },
  {
    name: "temperature",
    label: "Temperatura (°C)",
    placeholder: "24,3",
    helper: "Ideal entre 20 e 30 °C",
    inputMode: "decimal",
  },
  {
    name: "turbidity",
    label: "Turbidez (NTU)",
    placeholder: "48,5",
    helper: "Ideal até 40 NTU",
    inputMode: "decimal",
  },
  {
    name: "conductivity",
    label: "Condutividade (µS/cm)",
    placeholder: "312",
    helper: "Quanto menor, melhor",
    inputMode: "decimal",
  },
  {
    name: "tds",
    label: "TDS (ppm)",
    placeholder: "156",
    helper: "Ideal até 300 ppm",
    inputMode: "decimal",
  },
  {
    name: "heavyMetals",
    label: "Metais pesados (mg/L)",
    placeholder: "0,045",
    helper: "Ideal até 0,05 mg/L",
    inputMode: "decimal",
  },
];
