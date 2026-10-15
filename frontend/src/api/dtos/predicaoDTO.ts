import type { MetalSymbol } from "@/features/add-analysis/types/addAnalysis";

export interface MetalObservationDTO { name: MetalSymbol; value: number; unit: string }
export interface AmostraDTO {
  temperatura: number;
  ph: number;
  condutividade_eletrica: number;
  oxigenio_dissolvido: number;
  solidos_suspensos_totais?: number;
  carbono_organico_total?: number;
  fosforo_total?: number;
  metais_pesados?: MetalObservationDTO[];
}
export interface PredicaoDTO {
  id: number;
  coletaId: number;
  resultado: {
    riskLevel: number;
    riskLabel: string;
    riskReasons: string[];
    metalPredictions: { name: string; value: number; unit: string }[];
  };
}
