"use client";

import { useEffect, useState } from "react";
import { corpoHidricoServices } from "@/api/services/corpoHidricoServices";
import { coletaServices } from "@/api/services/coletaServices";
import { qualidadeServices } from "@/api/services/qualidadeServices";
import type { River } from "../types/river";
import { buildRivers } from "../utils/riverData";

async function readList(request: Promise<Response>): Promise<unknown[]> {
  const response = await request;
  if (!response.ok) throw new Error(`A API respondeu com erro ${response.status}.`);
  if (response.status === 204) return [];
  const data: unknown = await response.json();
  if (!Array.isArray(data)) throw new Error("A API retornou um formato de lista inesperado.");
  return data;
}

export function useWaterBodies() {
  const [attempt, setAttempt] = useState(0);
  const [state, setState] = useState<{
    rivers: River[];
    loading: boolean;
    error: string | null;
    warnings: string[];
  }>({ rivers: [], loading: true, error: null, warnings: [] });

  useEffect(() => {
    const controller = new AbortController();

    async function load() {
      const [bodies, collections, qualities] = await Promise.allSettled([
        readList(corpoHidricoServices.getAll(controller.signal)),
        readList(coletaServices.getAll(controller.signal)),
        readList(qualidadeServices.getAll(controller.signal)),
      ]);
      if (controller.signal.aborted) return;

      if (bodies.status === "rejected") {
        setState({ rivers: [], loading: false, error: "Não foi possível carregar os corpos hídricos. Tente novamente.", warnings: [] });
        return;
      }

      try {
        const result = buildRivers(bodies.value,
          collections.status === "fulfilled" ? collections.value : [],
          qualities.status === "fulfilled" ? qualities.value : []);
        if (collections.status === "rejected") result.warnings.push("Não foi possível carregar as coletas. As medições e os gráficos estão indisponíveis.");
        if (qualities.status === "rejected") result.warnings.push("Não foi possível carregar os índices de qualidade. Os valores de IQA estão indisponíveis.");
        setState({ ...result, loading: false, error: null });
      } catch {
        setState({ rivers: [], loading: false, error: "Os dados recebidos não puderam ser interpretados. Tente novamente.", warnings: [] });
      }
    }

    void load();
    return () => controller.abort();
  }, [attempt]);

  function retry() {
    setState({ rivers: [], loading: true, error: null, warnings: [] });
    setAttempt((current) => current + 1);
  }

  return { ...state, retry };
}
