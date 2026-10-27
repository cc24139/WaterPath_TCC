export interface AmostraDTO {
  temperatura: number;
  ph: number;
  condutividade_eletrica: number;
  oxigenio_dissolvido: number;
  solidos_suspensos_totais?: number;
  carbono_organico_total?: number;
  fosforo_total?: number;
  metais_pesados?: { name: string; value: number; unit: string }[];
}

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
