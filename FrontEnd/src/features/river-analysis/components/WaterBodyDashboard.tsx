"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { useGetById } from "@/api/hooks/useCorpoHidrico";
import { Button } from "@/components/ui/Button";
import MonitoringSkeleton from "./MonitoringSkeleton";
import WaterBodyNotFound from "./WaterBodyNotFound";

export function WaterBodyDashboard({ waterBodyId }: { waterBodyId: number }) {
  const { status, error, corpoHidrico, getCorpoHidricoById } = useGetById();
  const [attempt, setAttempt] = useState(0);

  useEffect(() => {
    const controller = new AbortController();
    void getCorpoHidricoById(waterBodyId, controller.signal);
    return () => controller.abort();
  }, [waterBodyId, attempt, getCorpoHidricoById]);

  if (status === "idle" || status === "loading") return <MonitoringSkeleton />;
  if (status === "not-found") return <WaterBodyNotFound />;
  if (status === "success" && corpoHidrico) {
    return <h1 className="font-heading text-xl font-bold">Dashboard do corpo hídrico {corpoHidrico.id}</h1>;
  }

  const title = status === "unauthenticated" ? "Entre para acessar o monitoramento"
    : status === "forbidden" ? "Acesso negado" : "Não foi possível carregar o monitoramento";
  const message = status === "unauthenticated" ? "Sua sessão está ausente ou expirou. Entre para consultar os dados."
    : status === "forbidden" ? "Você não tem permissão para consultar este corpo hídrico." : error;
  return (
    <section aria-labelledby="dashboard-state-title" className="space-y-4 rounded-xl border border-placeholder p-6">
      <h1 id="dashboard-state-title" className="font-heading text-xl font-bold">{title}</h1>
      <p role="alert" className="text-sm text-text-secondary">{message}</p>
      {status === "unauthenticated" && (
        <Link className="inline-block text-primary underline" href={`/login?next=${encodeURIComponent(`/water-bodies/${waterBodyId}/dashboard`)}`}>Entrar</Link>
      )}
      <div><Button onClick={() => setAttempt((value) => value + 1)}>Tentar novamente</Button></div>
      <Link className="inline-block text-primary underline" href="/water-bodies">Voltar aos corpos hídricos</Link>
    </section>
  );
}
