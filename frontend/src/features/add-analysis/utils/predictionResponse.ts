import type { PredicaoDTO } from "../../../api/dtos/predicaoDTO";

export function readPrediction(data: unknown, collectionId: number): PredicaoDTO {
  const prediction = data as Partial<PredicaoDTO> | null;
  const result = prediction?.resultado;
  if (!prediction || !Number.isInteger(prediction.id) || prediction.id! <= 0 || prediction.coletaId !== collectionId
    || !result || ![1, 2, 3].includes(result.riskLevel) || typeof result.riskLabel !== "string"
    || !Array.isArray(result.riskReasons) || !result.riskReasons.every((reason) => typeof reason === "string")
    || !Array.isArray(result.metalPredictions) || !result.metalPredictions.every((metal) => metal
      && typeof metal.name === "string" && typeof metal.unit === "string" && typeof metal.value === "number" && Number.isFinite(metal.value))) {
    throw new Error("A API retornou uma análise em formato inesperado.");
  }
  return prediction as PredicaoDTO;
}
