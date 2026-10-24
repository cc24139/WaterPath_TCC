"use client";

import Link from "next/link";
import { useState } from "react";
import { LuChevronLeft, LuChevronRight, LuDownload, LuMapPin, LuSearch } from "react-icons/lu";
import type { MonitoringColetaDTO } from "@/api/dtos/monitoringDTO";
import { Button } from "@/components/ui/Button";
import { Card } from "@/components/ui/Card";
import { RiskBadge } from "@/features/analysis-risk/components/RiskBadge";
import { useAnalysisSummaries } from "@/features/analysis-risk/hooks/useAnalysisSummaries";
import type { AnalysisSummary } from "@/features/analysis-risk/utils/analysisRisk";
import { collectionIqa } from "../../analysis/utils/analysisData";
import MonitoringSkeleton from "../../components/MonitoringSkeleton";
import WaterBodyNotFound from "../../components/WaterBodyNotFound";
import { useWaterBodyDashboard } from "../../dashboard/hooks/useWaterBodyDashboard";
import { filterCollections, formatMonitoringDate } from "../../dashboard/utils/dashboardData";
import { HistoryChart } from "./HistoryChart";
import { buildHistoryCsv, historyIndicators, historyMeasurement, historyValue } from "../utils/historyData";

const periods = [
  { value: "all", label: "Todo o período" },
  { value: "7", label: "7 dias" },
  { value: "30", label: "30 dias" },
  { value: "90", label: "90 dias" },
  { value: "365", label: "12 meses" },
] as const;
const pageSize = 8;

function HistoryRisk({ summary, pending, unavailable }: { summary?: AnalysisSummary; pending: boolean; unavailable: boolean }) {
  if (pending) return <span className="text-xs text-text-secondary">Carregando...</span>;
  return <RiskBadge level={summary?.risco?.level} unavailable={unavailable || (!!summary?.predicaoId && !summary.risco)} />;
}

function MeasurementCells({ collection }: { collection: MonitoringColetaDTO }) {
  return <>{historyIndicators.map((indicator) => <td key={indicator.key} className="whitespace-nowrap px-3 py-3.5 text-xs text-text-primary">{historyMeasurement(collection, indicator)}</td>)}</>;
}

export function WaterBodyHistory({ waterBodyId }: { waterBodyId: number }) {
  const { status, error, corpoHidrico, collections, collectionsLoading, collectionsError, updatedAt, retry } = useWaterBodyDashboard(waterBodyId);
  const [period, setPeriod] = useState<string>("all");
  const [selectedIndicator, setSelectedIndicator] = useState<string | null>(null);
  const [search, setSearch] = useState("");
  const [onlyMeasured, setOnlyMeasured] = useState(false);
  const [oldestFirst, setOldestFirst] = useState(false);
  const [page, setPage] = useState(1);

  const periodCollections = period === "all" ? collections : filterCollections(collections, Number(period), updatedAt ?? 0);
  const availableIndicators = historyIndicators.filter((indicator) => periodCollections.some((item) => historyValue(item, indicator) !== null));
  const indicator = historyIndicators.find((item) => item.key === selectedIndicator)
    ?? availableIndicators[0] ?? historyIndicators[0];
  const normalizedSearch = search.trim().toLocaleLowerCase("pt-BR");
  const filtered = periodCollections.filter((item) => {
    const matchesSearch = !normalizedSearch || String(item.id).includes(normalizedSearch)
      || formatMonitoringDate(item.dataHora).toLocaleLowerCase("pt-BR").includes(normalizedSearch);
    return matchesSearch && (!onlyMeasured || historyValue(item, indicator) !== null);
  });
  const ordered = oldestFirst ? [...filtered].reverse() : filtered;
  const pageCount = Math.max(1, Math.ceil(ordered.length / pageSize));
  const currentPage = Math.min(page, pageCount);
  const visible = ordered.slice((currentPage - 1) * pageSize, currentPage * pageSize);
  const latest = collections[0];
  const latestIqaCollection = collections.find((item) => collectionIqa(item) !== null);
  const riskQuery = new URLSearchParams({ corpoHidricoId: String(waterBodyId), pagina: "1", tamanhoPagina: String(pageSize) });
  visible.forEach((item) => riskQuery.append("coletaIds", String(item.id)));
  const riskState = useAnalysisSummaries(riskQuery.toString(), status === "success" && visible.length > 0);
  const risks = new Map(riskState.data?.itens.map((item) => [item.coletaId, item]));

  function exportMeasurements() {
    const url = URL.createObjectURL(new Blob([buildHistoryCsv(ordered)], { type: "text/csv;charset=utf-8" }));
    const anchor = document.createElement("a");
    anchor.href = url;
    anchor.download = `water-path-historico-${waterBodyId}-${period}.csv`;
    anchor.click();
    setTimeout(() => URL.revokeObjectURL(url), 1000);
  }

  if (status === "idle" || status === "loading") return <MonitoringSkeleton />;
  if (status === "not-found") return <WaterBodyNotFound />;
  if (status !== "success" || !corpoHidrico) return <Card className="space-y-4 !p-6">
    <h1 className="font-heading text-xl font-bold">{status === "unauthenticated" ? "Entre para acessar o histórico" : "Histórico indisponível"}</h1>
    <p role="alert" className="text-sm text-text-secondary">{error ?? "Não foi possível carregar este corpo hídrico."}</p>
    {status === "unauthenticated" && <Link href={`/login?next=${encodeURIComponent(`/water-bodies/${waterBodyId}/history`)}`} className="text-sm text-primary underline">Entrar</Link>}
    <div><Button onClick={retry}>Tentar novamente</Button></div>
  </Card>;

  return <div className="space-y-5">
    <header className="flex flex-col justify-between gap-4 xl:flex-row xl:items-end">
      <div className="min-w-0">
        <p className="mb-1 text-[11px] font-bold uppercase tracking-[0.16em] text-primary">Histórico de medições</p>
        <h1 className="break-words font-heading text-2xl font-bold sm:text-3xl">{corpoHidrico.nome}</h1>
        <p className="mt-1 flex items-center gap-1.5 text-xs text-text-secondary"><LuMapPin aria-hidden="true" />{corpoHidrico.localizacao}</p>
      </div>
      <Button variant="outline" className="w-full sm:w-auto" disabled={collectionsLoading || ordered.length === 0} onClick={exportMeasurements}><LuDownload aria-hidden="true" />Exportar medições</Button>
    </header>

    {collectionsError && <Card role="alert" className="flex flex-wrap items-center justify-between gap-3 !p-4 text-sm"><p>{collectionsError} {collections.length > 0 && "Os dados anteriores foram preservados."}</p><Button variant="outline" onClick={retry}>Tentar novamente</Button></Card>}
    {collectionsLoading ? <MonitoringSkeleton /> : collectionsError && !collections.length ? <Card><p className="text-sm text-text-secondary">A consulta não foi concluída. Tente novamente para visualizar as medições.</p></Card> : <>
      <div className="grid gap-3 sm:grid-cols-3">
        <Card className="border border-primary/10 !p-4 sm:!p-5"><p className="text-xs text-text-secondary">Coletas registradas</p><p className="mt-2 font-heading text-2xl font-bold text-text-primary">{collections.length}</p><p className="mt-1 text-xs text-text-secondary">{period === "all" ? "Em todo o histórico" : `${periodCollections.length} no período selecionado`}</p></Card>
        <Card className="border border-primary/10 !p-4 sm:!p-5"><p className="text-xs text-text-secondary">Última coleta</p><p className="mt-2 break-words font-heading text-sm font-bold text-text-primary sm:text-base">{latest ? formatMonitoringDate(latest.dataHora) : "Nenhuma coleta"}</p><p className="mt-1 text-xs text-text-secondary">Horário de Brasília</p></Card>
        <Card className="border border-primary/10 !p-4 sm:!p-5"><p className="text-xs text-text-secondary">Último IQA medido</p><p className="mt-2 font-heading text-2xl font-bold text-primary">{latestIqaCollection ? `${collectionIqa(latestIqaCollection)?.toLocaleString("pt-BR", { maximumFractionDigits: 2 })} pontos` : "Não medido"}</p><p className="mt-1 text-xs text-text-secondary">{latestIqaCollection ? formatMonitoringDate(latestIqaCollection.dataHora) : "O IQA só aparece quando consta na coleta."}</p></Card>
      </div>

      <Card className="!p-4 sm:!p-6">
        <div className="flex flex-col justify-between gap-4 lg:flex-row lg:items-start">
          <div><h2 className="font-heading text-base font-bold">Evolução de {indicator.label}</h2><p className="mt-1 text-xs text-text-secondary">Valores medidos ao longo do tempo · {indicator.unit || "escala de pH"}</p></div>
          <div className="flex flex-wrap gap-2" role="group" aria-label="Período do histórico">
            {periods.map((item) => <button key={item.value} type="button" onClick={() => { setPeriod(item.value); setPage(1); }} aria-pressed={period === item.value} className={`rounded-md border px-3 py-2 text-xs font-semibold transition-colors focus-visible:outline-2 focus-visible:outline-primary ${period === item.value ? "border-primary bg-primary text-white" : "border-placeholder bg-white text-text-secondary hover:border-primary hover:text-primary"}`}>{item.label}</button>)}
          </div>
        </div>
        <div className="mt-4 flex flex-wrap items-center justify-between gap-3 border-t border-placeholder/60 pt-4">
          <div className="flex items-center gap-2"><label htmlFor="history-indicator" className="text-xs font-semibold text-text-secondary">Indicador</label><select id="history-indicator" value={indicator.key} onChange={(event) => { setSelectedIndicator(event.target.value); setPage(1); }} className="h-10 max-w-full rounded-md border border-placeholder bg-white px-3 text-xs focus-visible:outline-primary">{historyIndicators.map((item) => <option key={item.key} value={item.key}>{item.label}</option>)}</select></div>
          <p className="text-xs text-text-secondary">{periodCollections.length} {periodCollections.length === 1 ? "coleta" : "coletas"} no período</p>
        </div>
        <div className="mt-3"><HistoryChart collections={periodCollections} indicator={indicator} /></div>
        <p className="mt-2 text-[11px] leading-relaxed text-text-secondary">O gráfico usa apenas valores válidos na unidade esperada. Coletas sem medição válida interrompem a linha. O IQA é exibido somente quando foi medido; risco da IA é uma classificação diferente.</p>
      </Card>

      <Card className="!p-4 sm:!p-6">
        <div className="flex flex-col justify-between gap-4 lg:flex-row lg:items-end">
          <div><h2 className="font-heading text-base font-bold">Histórico de coletas</h2><p className="mt-1 text-xs text-text-secondary">{filtered.length} {filtered.length === 1 ? "coleta encontrada" : "coletas encontradas"}</p></div>
          <div className="grid gap-3 sm:grid-cols-[minmax(180px,1fr)_auto] lg:min-w-[400px]">
            <div className="relative"><label htmlFor="history-search" className="sr-only">Buscar por data ou número da coleta</label><LuSearch aria-hidden="true" className="pointer-events-none absolute left-3 top-3.5 h-4 w-4 text-text-secondary" /><input id="history-search" type="search" value={search} onChange={(event) => { setSearch(event.target.value); setPage(1); }} placeholder="Data ou nº da coleta" className="h-11 w-full rounded-md border border-placeholder bg-white pl-9 pr-3 text-xs focus-visible:outline-primary" /></div>
            <div><label htmlFor="history-order" className="sr-only">Ordem das coletas</label><select id="history-order" value={oldestFirst ? "oldest" : "newest"} onChange={(event) => { setOldestFirst(event.target.value === "oldest"); setPage(1); }} className="h-11 w-full rounded-md border border-placeholder bg-white px-3 text-xs focus-visible:outline-primary"><option value="newest">Mais recentes</option><option value="oldest">Mais antigas</option></select></div>
          </div>
        </div>
        <label className="mt-4 inline-flex cursor-pointer items-center gap-2 text-xs text-text-secondary"><input type="checkbox" checked={onlyMeasured} onChange={(event) => { setOnlyMeasured(event.target.checked); setPage(1); }} className="accent-primary" />Somente coletas com {indicator.label}</label>
        {filtered.length === 0 ? <p role="status" className="py-12 text-center text-sm text-text-secondary">{collections.length === 0 ? "Nenhuma coleta registrada para este corpo hídrico." : "Nenhuma coleta corresponde aos filtros."}</p> : <>
          <div className="mt-4 space-y-3 md:hidden">{visible.map((collection) => <div key={collection.id} className="rounded-xl border border-placeholder/70 p-4">
            <div className="flex flex-wrap items-start justify-between gap-2"><div><p className="font-heading text-sm font-bold">{formatMonitoringDate(collection.dataHora)}</p><p className="mt-0.5 text-xs text-text-secondary">Coleta #{collection.id}</p></div><HistoryRisk summary={risks.get(collection.id)} pending={riskState.loading} unavailable={!!riskState.error || riskState.needsLogin} /></div>
            <dl className="mt-3 grid grid-cols-2 gap-x-4 gap-y-3 border-t border-placeholder/60 pt-3">{historyIndicators.map((item) => <div key={item.key}><dt className="text-[11px] text-text-secondary">{item.label}</dt><dd className="mt-0.5 text-xs font-semibold">{historyMeasurement(collection, item)}</dd></div>)}</dl>
          </div>)}</div>
          <div className="mt-4 hidden overflow-x-auto md:block"><table className="w-full min-w-[850px] text-left"><caption className="sr-only">Coletas do corpo hídrico no período selecionado. Horários de Brasília.</caption><thead><tr className="border-b border-placeholder bg-background/40 text-[11px] text-text-secondary"><th scope="col" className="rounded-l-lg px-3 py-3 font-semibold">Data e hora</th><th scope="col" className="px-3 py-3 font-semibold">Coleta</th>{historyIndicators.map((item) => <th key={item.key} scope="col" className="px-3 py-3 font-semibold">{item.label}</th>)}<th scope="col" className="rounded-r-lg px-3 py-3 font-semibold">Risco da IA</th></tr></thead><tbody>{visible.map((collection) => <tr key={collection.id} className="border-b border-placeholder/60 last:border-0 hover:bg-primary/[0.03]"><td className="whitespace-nowrap px-3 py-3.5 text-xs font-semibold">{formatMonitoringDate(collection.dataHora)}</td><td className="px-3 py-3.5 text-xs text-text-secondary">#{collection.id}</td><MeasurementCells collection={collection} /><td className="px-3 py-3.5"><HistoryRisk summary={risks.get(collection.id)} pending={riskState.loading} unavailable={!!riskState.error || riskState.needsLogin} /></td></tr>)}</tbody></table></div>
          <nav aria-label="Paginação das coletas" className="mt-5 flex flex-wrap items-center justify-between gap-3 border-t border-placeholder/60 pt-4"><Button variant="outline" className="h-10 !px-3" disabled={currentPage === 1} onClick={() => setPage(currentPage - 1)}><LuChevronLeft aria-hidden="true" />Anterior</Button><span className="text-xs text-text-secondary">Página {currentPage} de {pageCount}</span><Button variant="outline" className="h-10 !px-3" disabled={currentPage >= pageCount} onClick={() => setPage(currentPage + 1)}>Próxima<LuChevronRight aria-hidden="true" /></Button></nav>
        </>}
        {riskState.error && <div role="alert" className="mt-4 flex flex-wrap items-center justify-between gap-2 rounded-lg bg-background/60 p-3 text-xs text-text-secondary"><span>{riskState.error} As medições continuam disponíveis.</span><button type="button" onClick={riskState.retry} className="font-semibold text-primary underline">Atualizar riscos</button></div>}
        <p className="mt-4 text-[11px] leading-relaxed text-text-secondary">“—” indica dado indisponível; “&lt;” indica valor abaixo do limite de detecção. Risco da IA e IQA medido são informações distintas. O CSV contém as medições exibidas pelos filtros, em todas as páginas.</p>
      </Card>
    </>}
  </div>;
}
