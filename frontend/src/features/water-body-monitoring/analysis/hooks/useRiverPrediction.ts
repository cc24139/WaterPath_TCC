"use client";

import { useEffect, useState } from "react";
import type { PredicaoDTO } from "@/api/dtos/predicaoDTO";
import { predicaoServices } from "@/api/services/predicaoServices";
import { readPrediction } from "@/features/add-analysis/utils/predictionResponse";

type PredictionState = {
  key: string;
  prediction?: PredicaoDTO;
  imageUrl?: string;
  error?: string;
  imageError?: string;
};

export function useRiverPrediction(predictionId: number | null, collectionId: number | null) {
  const [attempt, setAttempt] = useState(0);
  const key = `${predictionId ?? ""}:${collectionId ?? ""}:${attempt}`;
  const [state, setState] = useState<PredictionState | null>(null);

  useEffect(() => {
    if (predictionId === null || collectionId === null) return;
    const controller = new AbortController();
    let objectUrl: string | undefined;

    async function load() {
      let prediction: PredicaoDTO;
      try {
        const response = await predicaoServices.getById(predictionId!, controller.signal);
        if (!response.ok) throw new Error("Não foi possível carregar o resultado completo da análise.");
        prediction = readPrediction(await response.json(), collectionId!);
        if (controller.signal.aborted) return;
        setState({ key, prediction });
      } catch (error) {
        if (!controller.signal.aborted) setState({ key, error: error instanceof Error ? error.message : "Falha ao consultar a análise." });
        return;
      }

      try {
        const response = await predicaoServices.getResultImage(predictionId!, controller.signal);
        if (!response.ok) throw new Error("Imagem anotada indisponível.");
        const blob = await response.blob();
        if (!blob.size || !["image/jpeg", "image/png"].includes(blob.type)) throw new Error("A imagem retornada não é válida.");
        if (controller.signal.aborted) return;
        objectUrl = URL.createObjectURL(blob);
        setState({ key, prediction, imageUrl: objectUrl });
      } catch (error) {
        if (!controller.signal.aborted) setState({ key, prediction,
          imageError: error instanceof Error ? error.message : "Imagem anotada indisponível." });
      }
    }

    void load();
    return () => { controller.abort(); if (objectUrl) URL.revokeObjectURL(objectUrl); };
  }, [predictionId, collectionId, key]);

  const current = state?.key === key ? state : null;
  return { prediction: current?.prediction, imageUrl: current?.imageUrl, error: current?.error,
    imageError: current?.imageError, loading: predictionId !== null && collectionId !== null && !current,
    retry: () => setAttempt((value) => value + 1) };
}
