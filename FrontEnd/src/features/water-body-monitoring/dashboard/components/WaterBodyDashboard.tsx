"use client";

import Link from "next/link";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { LuCalendar, LuDownload, LuDroplet, LuMapPin, LuTrendingUp } from "react-icons/lu";
import { useWaterBodyDashboard } from "../hooks/useWaterBodyDashboard";
import { buildMeasurementsCsv, dashboardPeriods, filterCollections, formatMonitoringDate } from "../utils/dashboardData";
import { DashboardPanel } from "./DashboardPanel";
import { DashboardEvolutionChart } from "./DashboardEvolutionChart";
import { CurrentParametersCard, RecentMeasurementsTable } from "./DashboardMeasurements";
import { Button } from "@/components/ui/Button";
import MonitoringSkeleton from "../../components/MonitoringSkeleton";
import WaterBodyNotFound from "../../components/WaterBodyNotFound";

export function WaterBodyDashboard({ waterBodyId }: { waterBodyId: number }) {
  const { status, error, corpoHidrico, collections, collectionsLoading, collectionsError, updatedAt, retry } = useWaterBodyDashboard(waterBodyId);
  const searchParams = useSearchParams();
  const router = useRouter();
  const pathname = usePathname();
  const requestedPeriod = searchParams.get("period");
  const period = dashboardPeriods.some((item) => item.value === requestedPeriod) ? requestedPeriod! : "30";
  const filtered = filterCollections(collections, Number(period), updatedAt ?? 0);
  const latest = collections[0];
  const basePath = `/water-bodies/${waterBodyId}`;

  function changePeriod(value: string) {
    const query = new URLSearchParams(searchParams.toString());
    query.set("period", value);
    router.replace(`${pathname}?${query}`, { scroll: false });
  }

  function exportMeasurements() {
    const url = URL.createObjectURL(new Blob([buildMeasurementsCsv(filtered)], { type: "text/csv;charset=utf-8" }));
    const anchor = document.createElement("a");
    anchor.href = url;
    anchor.download = `water-path-${waterBodyId}-${period}-dias.csv`;
    anchor.click();
    setTimeout(() => URL.revokeObjectURL(url), 1000);
  }

  if (status === "idle" || status === "loading") return <MonitoringSkeleton />;
  if (status === "not-found") return <WaterBodyNotFound />;
  if (status === "success" && corpoHidrico) {
    return (
      <div className="space-y-5">
        <header className="flex flex-col justify-between gap-4 xl:flex-row xl:items-center">
          <div className="min-w-0">
            <h1 className="break-words font-heading text-2xl font-bold sm:text-3xl">{corpoHidrico.nome}</h1>
            <div className="mt-1 flex flex-wrap items-center gap-2 text-xs text-text-secondary"><LuMapPin aria-hidden="true" /><span>{corpoHidrico.localizacao}</span><span aria-hidden="true">·</span><span>Sem classificação</span></div>
          </div>
          <div className="flex flex-wrap items-end gap-3">
            <p className="text-[11px] text-text-secondary">Dados consultados em<br /><span>{updatedAt ? formatMonitoringDate(updatedAt) : "—"}</span></p>
            <div className="min-w-0"><label htmlFor="dashboard-period" className="sr-only">Período do histórico</label><select id="dashboard-period" value={period} onChange={(event) => changePeriod(event.target.value)} className="h-12 rounded-md border border-placeholder bg-white px-3 text-xs focus-visible:outline-primary">{dashboardPeriods.map((item) => <option key={item.value} value={item.value}>Últimos {item.label}</option>)}</select></div>
            <Button variant="outline" disabled={collectionsLoading || !!collectionsError || filtered.length === 0} onClick={exportMeasurements}><LuDownload aria-hidden="true" />Exportar CSV</Button>
          </div>
        </header>
        {collectionsError && <div role="alert" className="flex flex-wrap items-center justify-between gap-3 rounded-lg border border-placeholder bg-white p-4 text-sm"><p>{collectionsError} {collections.length > 0 && "Os dados anteriores foram preservados."}</p><Button variant="outline" onClick={retry}>Tentar novamente</Button></div>}
        {collectionsLoading ? <MonitoringSkeleton /> : collectionsError && collections.length === 0 ? <DashboardPanel title="Medições indisponíveis"><p className="text-sm text-text-secondary">A consulta não foi concluída. Tente novamente para exibir os parâmetros e o histórico.</p></DashboardPanel> : <>
          {!collectionsError && collections.length === 0 && <p role="status" className="rounded-lg bg-primary/5 p-4 text-sm">Nenhuma coleta registrada para este corpo hídrico.</p>}
          <div className="grid gap-4 sm:grid-cols-2 xl:grid-cols-4">
            <DashboardPanel title="IQA atual"><strong className="text-3xl text-primary">—</strong><div aria-hidden="true" className="mt-3 h-2 w-full rounded-full bg-placeholder/40" /><p className="mt-2 text-xs text-text-secondary">Índice atual indisponível</p></DashboardPanel>
            <DashboardPanel title="Status"><div className="flex items-center gap-3"><LuDroplet aria-hidden="true" className="h-10 w-10 rounded-full bg-primary/10 p-2 text-primary" /><div><strong className="text-sm text-text-secondary">Sem classificação</strong><p className="mt-1 text-xs text-text-secondary">Aguardando índice de qualidade</p></div></div></DashboardPanel>
            <DashboardPanel title="Última coleta"><div className="flex items-center gap-3"><LuCalendar aria-hidden="true" className="h-10 w-10 shrink-0 rounded-full bg-background p-2 text-text-secondary" /><strong className="text-sm">{latest ? formatMonitoringDate(latest.dataHora) : "—"}</strong></div><p className="mt-2 text-xs text-text-secondary">{latest ? "Horário de Brasília" : collectionsError ? "Consulta indisponível" : "Nenhuma coleta registrada"}</p></DashboardPanel>
            <DashboardPanel title={`Tendência (${dashboardPeriods.find((item) => item.value === period)?.label})`}><div className="flex items-center gap-3"><LuTrendingUp aria-hidden="true" className="h-10 w-10 rounded-full bg-background p-2 text-text-secondary" /><div><strong className="text-sm text-text-secondary">Indisponível</strong><p className="mt-1 text-xs text-text-secondary">Sem série temporal de IQA</p></div></div></DashboardPanel>
          </div>
          <DashboardEvolutionChart collections={filtered} period={period} onPeriodChange={changePeriod} />
          <div className="grid items-stretch gap-4 xl:grid-cols-[minmax(0,0.85fr)_minmax(0,1.15fr)]">
            <CurrentParametersCard latest={latest} basePath={basePath} />
            <DashboardPanel title="Diagnóstico resumido" action={{ label: "Ver análise e recomendações", href: `${basePath}/analysis` }}>
              <div className="flex min-h-28 items-center gap-4"><LuDroplet aria-hidden="true" className="h-14 w-14 shrink-0 rounded-full bg-primary/5 p-4 text-primary" /><p className="text-sm leading-relaxed text-text-secondary">Ainda não há um diagnóstico disponível para exibir. As medições são apresentadas sem atribuir níveis de risco ou recomendações automaticamente.</p></div>
            </DashboardPanel>
          </div>
          <RecentMeasurementsTable collections={filtered} basePath={basePath} />
        </>}
      </div>
    );
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
      <div><Button onClick={retry}>Tentar novamente</Button></div>
      <Link className="inline-block text-primary underline" href="/water-bodies">Voltar aos corpos hídricos</Link>
    </section>
  );
}
