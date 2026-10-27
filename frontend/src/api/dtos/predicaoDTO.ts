export interface PredicaoDTO {
  id: number;
  coletaId: number;
  resultado: {
    riskLevel: number;
    riskLabel: string;
    riskReasons: string[];
    metalPredictions: { name: string; value: number; unit: string }[];
    measuredMetals?: { name: string; value: number; unit: string }[];
    estimatedMetals?: { name: string; value: number; unit: string }[];
    totalObjects?: number;
    detections?: { className: string; count: number; interpretation?: string }[];
  };
}
