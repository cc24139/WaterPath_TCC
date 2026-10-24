import type { AddAnalysisFormState, AnalysisFieldConfig, MetalSymbol } from "../types/addAnalysis";

export const indicatorFields: AnalysisFieldConfig[] = [
  { name: "temperatura", label: "Temperatura", unit: "°C", measurementCode: "Temperatura", numericCode: 0, placeholder: "Ex.: 24,3", required: true },
  { name: "ph", label: "pH", unit: "pH", measurementCode: "Ph", numericCode: 1, placeholder: "Ex.: 7,2", required: true },
  { name: "condutividade_eletrica", label: "Condutividade elétrica", unit: "µS/cm", measurementCode: "CondutividadeEletrica", numericCode: 4, placeholder: "Ex.: 312", required: true },
  { name: "oxigenio_dissolvido", label: "Oxigênio dissolvido", unit: "mg/L", measurementCode: "OxigenioDissolvido", numericCode: 2, placeholder: "Ex.: 6,5", required: true },
  { name: "solidos_suspensos_totais", label: "Sólidos suspensos totais", unit: "mg/L", measurementCode: "SolidosSuspensosTotais", numericCode: 5, placeholder: "Ex.: 12", required: false },
  { name: "carbono_organico_total", label: "Carbono orgânico total", unit: "mg/L", measurementCode: "CarbonoOrganicoTotal", numericCode: 7, placeholder: "Ex.: 3,5", required: false },
  { name: "fosforo_total", label: "Fósforo total", unit: "µg P/L", measurementCode: "FosforoTotal", numericCode: 12, placeholder: "Ex.: 25", required: false },
];

export const metalOptions: { symbol: MetalSymbol; label: string; unit: string }[] = [
  { symbol: "Fe", label: "Ferro", unit: "mg/L" },
  { symbol: "Mn", label: "Manganês", unit: "mg/L" },
  { symbol: "Cr", label: "Cromo", unit: "µg/L" },
  { symbol: "Ni", label: "Níquel", unit: "µg/L" },
  { symbol: "Cu", label: "Cobre", unit: "µg/L" },
  { symbol: "Zn", label: "Zinco", unit: "µg/L" },
  { symbol: "Cd", label: "Cádmio", unit: "µg/L" },
  { symbol: "Pb", label: "Chumbo", unit: "µg/L" },
];

export const initialAddAnalysisForm: AddAnalysisFormState = {
  waterBody: "", analysisDate: "", temperatura: "", ph: "", condutividade_eletrica: "",
  oxigenio_dissolvido: "", solidos_suspensos_totais: "", carbono_organico_total: "", fosforo_total: "", metals: {},
};
