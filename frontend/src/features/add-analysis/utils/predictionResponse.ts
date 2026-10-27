import type { PredicaoDTO } from "../../../api/dtos/predicaoDTO";
import { isRiskLevel } from "../../analysis-risk/utils/analysisRisk.ts";

const validMetals = (metals: unknown) => Array.isArray(metals) && metals.every((metal) => metal
  && typeof metal.name === "string" && typeof metal.unit === "string" && typeof metal.value === "number" && Number.isFinite(metal.value));

export function readPrediction(data: unknown, collectionId: number): PredicaoDTO {
  const prediction = data as Partial<PredicaoDTO> | null;
  const result = prediction?.resultado;
  if (!prediction || !Number.isInteger(prediction.id) || prediction.id! <= 0 || prediction.coletaId !== collectionId
    || !result || !isRiskLevel(result.riskLevel) || typeof result.riskLabel !== "string"
    || !Array.isArray(result.riskReasons) || !result.riskReasons.every((reason) => typeof reason === "string")
    || !validMetals(result.metalPredictions)
    || (result.measuredMetals !== undefined && !validMetals(result.measuredMetals))
    || (result.estimatedMetals !== undefined && !validMetals(result.estimatedMetals))
    || (result.totalObjects !== undefined && (!Number.isSafeInteger(result.totalObjects) || result.totalObjects < 0))
    || (result.detections !== undefined && (!Array.isArray(result.detections)
      || !result.detections.every((detection) => detection && typeof detection.className === "string"
        && Number.isSafeInteger(detection.count) && detection.count >= 0
        && (detection.interpretation === undefined || typeof detection.interpretation === "string"))))) {
    throw new Error("A API retornou uma análise em formato inesperado.");
  }
  return prediction as PredicaoDTO;
}
