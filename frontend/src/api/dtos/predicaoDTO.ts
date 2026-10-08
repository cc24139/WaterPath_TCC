import type { IndicatorName, MetalSymbol } from "@/features/add-analysis/types/addAnalysis";

export interface MetalObservationDTO { name: MetalSymbol; value: number; unit: string }
export type AmostraDTO = Partial<Record<IndicatorName, number>> & { metais_pesados?: MetalObservationDTO[] };
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
