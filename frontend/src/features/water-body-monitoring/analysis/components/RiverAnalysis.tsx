"use client";

/* eslint-disable @next/next/no-img-element -- The annotated image is an authenticated object URL, not a public Next image. */

import Link from "next/link";
import { useState } from "react";
import { LuCalendarDays, LuChartNoAxesCombined, LuImageOff, LuMapPin, LuRefreshCw, LuTriangleAlert } from "react-icons/lu";
import { Button } from "@/components/ui/Button";
import { Card } from "@/components/ui/Card";
import { RiskBadge } from "@/features/analysis-risk/components/RiskBadge";
import { useAnalysisSummaries } from "@/features/analysis-risk/hooks/useAnalysisSummaries";
import { isRiskLevel } from "@/features/analysis-risk/utils/analysisRisk";
import { DashboardPanel } from "../../dashboard/components/DashboardPanel";
import { formatMonitoringDate } from "../../dashboard/utils/dashboardData";
import { useWaterBodyDashboard } from "../../dashboard/hooks/useWaterBodyDashboard";
import MonitoringSkeleton from "../../components/MonitoringSkeleton";
import WaterBodyNotFound from "../../components/WaterBodyNotFound";
import { useRiverPrediction } from "../hooks/useRiverPrediction";
import { collectionIqa } from "../utils/analysisData";
import { RiverConditionCards } from "./RiverConditionCards";
import { RiverInsightListCard } from "./RiverInsightListCard";
import { AnalysisIndicatorsChart } from "./AnalysisIndicatorsChart";
import { RiverOverview } from "./RiverOverview";

export function RiverAnalysis({ waterBodyId }: { waterBodyId: number }) {
  const { status, error, corpoHidrico, collections, collectionsLoading, collectionsError, updatedAt, retry } = useWaterBodyDashboard(waterBodyId);
  const [activeView, setActiveView] = useState<"panorama" | "individual">("panorama");
  const [selectedCollectionId, setSelectedCollectionId] = useState<number | null>(null);
  const basePath = `/water-bodies/${waterBodyId}`;
  const query = new URLSearchParams({ corpoHidricoId: String(waterBodyId), pagina: "1", tamanhoPagina: "100" });
  const summaries = useAnalysisSummaries(query.toString(), status === "success" && activeView === "individual");
  const analyzed = summaries.data?.itens.filter((item) => item.predicaoId !== null) ?? [];
  const selected = analyzed.find((item) => item.coletaId === selectedCollectionId) ?? analyzed[0];
  const selectedCollection = collections.find((item) => item.id === selected?.coletaId);
  const result = useRiverPrediction(activeView === "individual" ? selected?.predicaoId ?? null : null,
    activeView === "individual" ? selected?.coletaId ?? null : null);
  const riskLevel = result.prediction?.resultado.riskLevel ?? selected?.risco?.level;
  const reasons = result.prediction?.resultado.riskReasons ?? selected?.risco?.reasons ?? [];
  const detections = result.prediction?.resultado.detections ?? [];
  const metals = result.prediction?.resultado.metalPredictions;

  if (status === "idle" || status === "loading") return <MonitoringSkeleton />;
  if (status === "not-found") return <WaterBodyNotFound />;
  if (status !== "success" || !corpoHidrico) {
    return <Card className="space-y-4 !p-6">
      <h1 className="font-heading text-xl font-bold">{status === "unauthenticated" ? "Entre para acessar a análise" : "Análise indisponível"}</h1>
      <p role="alert" className="text-sm text-text-secondary">{error ?? "Não foi possível carregar este corpo hídrico."}</p>
      {status === "unauthenticated" && <Link href={`/login?next=${encodeURIComponent(`${basePath}/analysis`)}`} className="text-sm text-primary underline">Entrar</Link>}
      <div><Button onClick={retry}>Tentar novamente</Button></div>
    </Card>;
  }

  const problemItems = reasons.length ? reasons.map((reason, index) => ({
    title: `Motivo ${index + 1}`, description: reason, colorClass: "text-warning",
    icon: <LuTriangleAlert className="h-5 w-5" aria-hidden="true" />,
  })) : [{ title: selected ? "Sem motivo adicional" : summaries.error ? "Consulta indisponível" : "Sem análise integrada",
    description: selected ? "O resultado não trouxe outros motivos de risco." : summaries.error
      ? "Não foi possível consultar os motivos do risco." : "Nenhuma coleta com resultado da IA foi encontrada nesta lista.",
    colorClass: "text-text-secondary", icon: <LuTriangleAlert className="h-5 w-5" aria-hidden="true" /> }];
  const actionItems = [
    { title: "Acompanhar o histórico", description: "Compare medições e riscos registrados em outras coletas.",
      colorClass: "text-primary", icon: <LuChartNoAxesCombined className="h-5 w-5" aria-hidden="true" /> },
    { title: "Registrar nova coleta", description: "Uma nova análise da IA exige medições e uma imagem da coleta.",
      colorClass: "text-primary", icon: <LuCalendarDays className="h-5 w-5" aria-hidden="true" /> },
  ];

  return <div className="space-y-5">
    <header className="flex flex-col justify-between gap-4 xl:flex-row xl:items-end">
      <div className="min-w-0">
        <p className="mb-1 text-[11px] font-bold uppercase tracking-[0.16em] text-primary">Análise da qualidade da água</p>
        <h1 className="break-words font-heading text-2xl font-bold sm:text-3xl">{corpoHidrico.nome}</h1>
        <p className="mt-1 flex items-center gap-1.5 text-xs text-text-secondary"><LuMapPin aria-hidden="true" />{corpoHidrico.localizacao}</p>
      </div>
      <div className="flex flex-wrap items-end gap-3">
        {activeView === "individual" && <div className="min-w-0">
          <label htmlFor="analysis-collection" className="mb-1 block text-xs font-medium text-text-secondary">Coleta analisada</label>
          <select id="analysis-collection" value={selected?.coletaId ?? ""} disabled={!analyzed.length}
            onChange={(event) => setSelectedCollectionId(Number(event.target.value))}
            className="h-12 max-w-full rounded-md border border-placeholder bg-white px-3 text-xs focus-visible:outline-primary disabled:opacity-65">
            {!analyzed.length && <option value="">Nenhuma análise disponível</option>}
            {analyzed.map((item) => <option key={item.coletaId} value={item.coletaId}>{formatMonitoringDate(item.dataHora)} · Coleta #{item.coletaId}</option>)}
          </select>
        </div>}
        <Button variant="outline" onClick={() => { retry(); if (activeView === "individual") { summaries.retry(); result.retry(); } }}><LuRefreshCw aria-hidden="true" />Atualizar dados</Button>
      </div>
    </header>

    <div role="group" aria-label="Tipo de análise" className="flex w-full flex-wrap gap-2 rounded-xl bg-white p-1.5 shadow-default sm:w-fit">
      {([{"value": "panorama", "label": "Panorama do rio"}, {"value": "individual", "label": "Coleta individual"}] as const).map((view) =>
        <button key={view.value} type="button" aria-pressed={activeView === view.value} onClick={() => setActiveView(view.value)}
          className={`min-h-10 flex-1 rounded-lg px-4 py-2 text-sm font-semibold transition-colors focus-visible:outline-2 focus-visible:outline-primary sm:flex-none ${activeView === view.value ? "bg-primary text-white" : "text-text-secondary hover:bg-primary/5 hover:text-primary"}`}>
          {view.label}
        </button>)}
    </div>

    {collectionsError && <Card role="alert" className="text-sm text-text-secondary">{collectionsError}</Card>}
    {activeView === "individual" && summaries.error && <Card role="alert" className="text-sm text-text-secondary">{summaries.error}</Card>}
    {activeView === "individual" && summaries.data && summaries.data.total > summaries.data.itens.length && <p className="text-xs text-text-secondary">A seleção mostra as 100 coletas mais recentes. Consulte o histórico para encontrar coletas anteriores.</p>}
    {activeView === "panorama" ? collectionsLoading ? <MonitoringSkeleton />
      : collectionsError && !collections.length ? <Card><p className="text-sm text-text-secondary">A consulta não foi concluída. Tente novamente para visualizar o panorama.</p></Card>
      : <RiverOverview collections={collections} until={updatedAt} basePath={basePath} />
    : collectionsLoading || summaries.loading ? <MonitoringSkeleton /> : <>
      <Card className="!p-4 sm:!p-5">
        <h2 className="mb-3 font-heading text-sm font-bold">Visualização do rio</h2>
        <div className="flex min-h-56 items-center justify-center overflow-hidden rounded-xl bg-background/60 sm:min-h-72">
          {result.imageUrl ? <img src={result.imageUrl} alt={`Imagem da coleta #${selected?.coletaId} com marcações produzidas pela IA`} className="max-h-[440px] w-full object-contain" />
            : <div className="flex flex-col items-center gap-3 px-6 py-12 text-center text-sm text-text-secondary"><LuImageOff className="h-10 w-10 text-primary/50" aria-hidden="true" />
              <p>{result.loading || (selected && !result.prediction && !result.error) ? "Carregando imagem da análise..."
                : result.imageError ?? (selected || summaries.error ? "Imagem da análise indisponível." : "Ainda não há imagem de análise integrada para este corpo hídrico.")}</p></div>}
        </div>
        <p className="mt-3 text-xs text-text-secondary">{result.imageUrl ? `Imagem anotada pela IA da coleta #${selected?.coletaId}. As marcações representam detecções visuais, não pontos geográficos.` : "Uma imagem aparece aqui quando uma análise integrada está disponível."}</p>
      </Card>

      <RiverConditionCards riskLevel={riskLevel} iqa={collectionIqa(selectedCollection)} metalCount={metals?.length} collectedAt={selected?.dataHora} />

      <div className="grid items-stretch gap-4 xl:grid-cols-[minmax(0,1.7fr)_minmax(280px,1fr)]">
        <DashboardPanel title="Análise inteligente">
          {selected ? <div className="space-y-3 text-sm text-text-secondary">
            <div className="flex flex-wrap items-center gap-3"><RiskBadge level={riskLevel} unavailable={!!selected.predicaoId && !isRiskLevel(riskLevel)} />
              <span>Análise #{selected.predicaoId} · {selected.analisadaEm ? formatMonitoringDate(selected.analisadaEm) : "data indisponível"}</span></div>
            <p>{isRiskLevel(riskLevel) ? "Classificação produzida pela regra de risco da IA para esta coleta. Consulte os motivos informados ao lado e os dados detalhados abaixo." : "O resultado de risco desta análise não está disponível."}</p>
            {result.error && <p role="alert">{result.error} O resumo de risco pode continuar disponível.</p>}
            {result.prediction && <details id="analysis-details" className="rounded-lg border border-placeholder p-3">
              <summary className="cursor-pointer font-semibold text-primary">Ver objetos e estimativas de metais</summary>
              <div className="mt-3 grid gap-4 sm:grid-cols-2">
                <div><h3 className="font-semibold text-text-primary">Detecções visuais</h3>
                  <p className="mt-1">{result.prediction.resultado.totalObjects ?? 0} objeto(s) detectado(s).</p>
                  {!!detections.length && <ul className="mt-2 list-disc space-y-1 pl-5">{detections.map((item, index) => <li key={`${item.className}-${index}`}>{item.className}: {item.count}</li>)}</ul>}</div>
                <div><h3 className="font-semibold text-text-primary">Metais estimados</h3>
                  {metals?.length ? <ul className="mt-2 grid grid-cols-2 gap-x-3 gap-y-1">{metals.map((metal) => <li key={metal.name}>{metal.name}: {metal.value.toLocaleString("pt-BR", { maximumFractionDigits: 3 })} {metal.unit}</li>)}</ul>
                    : <p className="mt-1">Sem estimativas disponíveis.</p>}
                  <p className="mt-2 text-xs">Estimativas não substituem medições laboratoriais.</p></div>
              </div>
            </details>}
          </div> : <p className="text-sm text-text-secondary">{summaries.error ? "Não foi possível consultar as análises. Tente atualizar os dados." : "Nenhuma análise integrada registrada. Cadastre uma coleta com imagem para gerar um resultado."}</p>}
        </DashboardPanel>
        <RiverInsightListCard title="Motivos do risco" items={problemItems} actionLabel="Ver histórico" actionHref={`${basePath}/history`} />
        <AnalysisIndicatorsChart collections={collections} until={updatedAt} />
        <RiverInsightListCard title="Ações disponíveis" items={actionItems} actionLabel="Registrar nova análise" actionHref="/add-analysis" />
      </div>
      <p className="text-xs text-text-secondary">O risco da IA, o IQA medido e as estimativas de metais são informações distintas. Esta tela não produz previsão futura nem recomendações automáticas.</p>
    </>}
  </div>;
}
