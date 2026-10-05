"use client";

import { useEffect, useState } from "react";
import { useGetById } from "@/api/hooks/useCorpoHidrico";
import { coletaServices } from "@/api/services/coletaServices";
import type { MonitoringColetaDTO } from "@/api/dtos/monitoringDTO";
import { readDashboardCollections } from "../utils/dashboardData";

export function useWaterBodyDashboard(waterBodyId: number) {
  const body = useGetById();
  const { getCorpoHidricoById } = body;
  const [attempt, setAttempt] = useState(0);
  const [collections, setCollections] = useState<MonitoringColetaDTO[]>([]);
  const [collectionsLoading, setCollectionsLoading] = useState(true);
  const [collectionsError, setCollectionsError] = useState<string | null>(null);
  const [updatedAt, setUpdatedAt] = useState<number | null>(null);

  useEffect(() => {
    const controller = new AbortController();
    async function load() {
      setCollectionsLoading(true);
      setCollectionsError(null);
      await getCorpoHidricoById(waterBodyId, controller.signal);
      if (controller.signal.aborted) return;
      try {
        const response = await coletaServices.getByWaterBody(waterBodyId, controller.signal);
        if (response.status === 401) {
          if (!controller.signal.aborted) setCollectionsError("Entre novamente para consultar as coletas.");
          return;
        }
        if (response.status === 403) {
          if (!controller.signal.aborted) setCollectionsError("Você não tem permissão para consultar as coletas.");
          return;
        }
        if (!response.ok) throw new Error("Não foi possível carregar as coletas. Tente novamente.");
        const result = readDashboardCollections(await response.json(), waterBodyId);
        if (!controller.signal.aborted) { setCollections(result); setUpdatedAt(Date.now()); }
      } catch {
        if (!controller.signal.aborted) setCollectionsError("Não foi possível carregar as coletas. Tente novamente.");
      } finally {
        if (!controller.signal.aborted) setCollectionsLoading(false);
      }
    }
    void load();
    return () => controller.abort();
  }, [waterBodyId, attempt, getCorpoHidricoById]);

  return { ...body, collections, collectionsLoading, collectionsError, updatedAt, retry: () => setAttempt((value) => value + 1) };
}
