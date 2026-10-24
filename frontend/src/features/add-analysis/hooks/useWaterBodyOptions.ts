"use client";

import { useEffect, useState } from "react";
import { corpoHidricoServices } from "@/api/services/corpoHidricoServices";
import { useAuthSession } from "@/features/auth/hooks/useAuthSession";
import type { SelectOption } from "../types/addAnalysis";
import { canAnalyzeWaterBody } from "../utils/waterBodyAccess";

export function useWaterBodyOptions() {
  const { user, isReady } = useAuthSession();
  const [attempt, setAttempt] = useState(0);
  const userId = user?.id;
  const key = `${userId ?? ""}:${attempt}`;
  const [state, setState] = useState<{
    key: string;
    options: SelectOption[];
    error: string | null;
  } | null>(null);

  useEffect(() => {
    if (!userId) return;
    const currentUserId = userId;
    const controller = new AbortController();

    async function load() {
      try {
        const response = await corpoHidricoServices.getAll(controller.signal);
        if (!response.ok) throw new Error("Falha ao buscar corpos hídricos.");
        const data: unknown = response.status === 204 ? [] : await response.json();
        if (!Array.isArray(data)) throw new Error("Formato de resposta inválido.");

        const options = new Map<string, SelectOption>();
        for (const item of data) {
          if (!item || typeof item !== "object") throw new Error("Registro inválido.");
          const body = item as Record<string, unknown>;
          if (!canAnalyzeWaterBody(body, currentUserId)) continue;
          const id = typeof body.id === "number" || typeof body.id === "string"
            ? Number(body.id) : NaN;
          if (!Number.isSafeInteger(id) || id <= 0 || typeof body.nome !== "string" || !body.nome.trim()) {
            throw new Error("Corpo hídrico sem ID ou nome válido.");
          }
          const location = typeof body.localizacao === "string" ? body.localizacao.trim() : "";
          const value = String(id);
          options.set(value, {
            value,
            label: location ? `${body.nome.trim()} — ${location}` : body.nome.trim(),
          });
        }

        if (!controller.signal.aborted) {
          setState({
            key,
            options: [...options.values()].sort((a, b) => a.label.localeCompare(b.label, "pt-BR")),
            error: null,
          });
        }
      } catch {
        if (!controller.signal.aborted) {
          setState({
            key,
            options: [],
            error: "Não foi possível carregar os corpos hídricos. Tente novamente.",
          });
        }
      }
    }

    void load();
    return () => controller.abort();
  }, [key, userId]);

  function retry() {
    setAttempt((current) => current + 1);
  }

  const current = state?.key === key ? state : null;
  return { options: current?.options ?? [], error: current?.error ?? null,
    loading: !isReady || (Boolean(userId) && !current), retry };
}
