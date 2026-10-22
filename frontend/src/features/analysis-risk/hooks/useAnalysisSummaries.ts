"use client";

import { useEffect, useState } from "react";
import { apiFetch } from "@/api/apiFetch";
import { routes } from "@/api/routes";
import { useAuthSession } from "@/features/auth/hooks/useAuthSession";
import { readAnalysisSummaries, type AnalysisSummaryPage } from "../utils/analysisRisk";

export function useAnalysisSummaries(query: string, enabled = true) {
  const { user, isReady } = useAuthSession();
  const [attempt, setAttempt] = useState(0);
  const key = `${user?.id ?? ""}:${query}:${attempt}`;
  const [state, setState] = useState<{ key: string; data?: AnalysisSummaryPage; error?: string } | null>(null);
  useEffect(() => {
    if (!enabled || !user) return;
    const controller = new AbortController();
    async function load() {
      try {
        const response = await apiFetch(`${routes.predicoes}resumos?${query}`, { signal: controller.signal }, { redirectOnUnauthorized: false });
        if (response.status === 401) throw new Error("Entre novamente para consultar as análises.");
        if (response.status === 403) throw new Error("Você não tem permissão para consultar estas análises.");
        if (response.status === 404) throw new Error("A listagem de análises ainda não está disponível neste ambiente.");
        if (!response.ok) throw new Error("Não foi possível carregar as análises. Tente novamente.");
        const data = readAnalysisSummaries(await response.json());
        if (!controller.signal.aborted) setState({ key, data });
      } catch (error) {
        if (!controller.signal.aborted) setState({ key, error: error instanceof Error ? error.message : "Falha ao consultar análises." });
      }
    }
    void load();
    return () => controller.abort();
  }, [query, enabled, key, user]);
  const current = state?.key === key ? state : null;
  return { data: current?.data, error: current?.error, loading: enabled && (!isReady || (!!user && !current)),
    needsLogin: isReady && !user, retry: () => setAttempt(value => value + 1) };
}
