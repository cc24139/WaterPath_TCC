"use client";

import Link from "next/link";
import { useState } from "react";
import { LuSearch, LuChevronLeft, LuChevronRight } from "react-icons/lu";
import { Button } from "@/components/ui/Button";
import { Card } from "@/components/ui/Card";
import { formatMonitoringDate } from "@/features/water-body-monitoring/dashboard/utils/dashboardData";
import { useAnalysisSummaries } from "../hooks/useAnalysisSummaries";
import { RiskBadge } from "./RiskBadge";

export function AnalysisHistory({ waterBodyId }: { waterBodyId?: number }) {
  const [search, setSearch] = useState("");
  const [submittedSearch, setSubmittedSearch] = useState("");
  const [oldest, setOldest] = useState(false);
  const [page, setPage] = useState(1);
  const query = new URLSearchParams({ pagina: String(page), tamanhoPagina: "10", busca: submittedSearch,
    antigasPrimeiro: String(oldest), somenteMinhas: String(!waterBodyId) });
  if (waterBodyId) query.set("corpoHidricoId", String(waterBodyId));
  const { data, error, loading, needsLogin, retry } = useAnalysisSummaries(query.toString());
  const totalPages = Math.max(1, Math.ceil((data?.total ?? 0) / 10));
  return <div className="space-y-5">
    <header className="flex flex-wrap items-start justify-between gap-4">
      <div><h1 className="font-heading text-2xl font-bold text-text-primary sm:text-3xl">{waterBodyId ? "Histórico de análises" : "Minhas análises"}</h1>
        <p className="mt-2 text-sm text-text-secondary">{waterBodyId ? "Coletas e resultados de risco deste corpo hídrico." : "Coletas cadastradas por você e seus resultados de risco."}</p></div>
      <Link href="/add-analysis" className="rounded-md bg-primary px-5 py-3 text-sm font-bold text-white">Nova análise</Link>
    </header>
    <Card>
      <form className="flex flex-wrap items-end gap-3" onSubmit={event => { event.preventDefault(); setSubmittedSearch(search.trim()); setPage(1); }}>
        <div className="min-w-0 flex-1"><label htmlFor="risk-search" className="mb-1 block text-xs font-semibold">Corpo hídrico ou localização</label>
          <input id="risk-search" type="search" maxLength={100} value={search} onChange={event => setSearch(event.target.value)} className="h-12 w-full rounded-md border border-placeholder px-3 text-sm focus:outline-primary" placeholder="Buscar análises..." /></div>
        <Button type="submit" variant="outline"><LuSearch aria-hidden="true" />Buscar</Button>
        <div><label htmlFor="risk-order" className="mb-1 block text-xs font-semibold">Data da coleta</label><select id="risk-order" value={String(oldest)} onChange={event => { setOldest(event.target.value === "true"); setPage(1); }} className="h-12 rounded-md border border-placeholder bg-white px-3 text-sm focus:outline-primary"><option value="false">Mais recentes</option><option value="true">Mais antigas</option></select></div>
      </form>
    </Card>
    {needsLogin ? <Card><p className="text-sm">Entre para consultar as análises.</p><Link href="/login" className="text-primary underline">Entrar</Link></Card>
      : loading ? <Card><p role="status" className="py-8 text-center text-sm text-text-secondary">Carregando análises...</p></Card>
      : error ? <Card><p role="alert" className="mb-3 text-sm">{error}</p><Button variant="outline" onClick={retry}>Tentar novamente</Button></Card>
      : data && <Card>
        <p className="mb-4 text-sm text-text-secondary">{data.total} {data.total === 1 ? "coleta encontrada" : "coletas encontradas"}. Cada coleta mostra sua última análise integrada. Horários de Brasília.</p>
        {!data.itens.length ? <p role="status" className="py-8 text-center text-text-secondary">{submittedSearch ? "Nenhuma coleta corresponde à busca." : "Nenhuma coleta nesta página."}</p>
          : <ul className="divide-y divide-placeholder">{data.itens.map(item => <li key={item.coletaId} id={`coleta-${item.coletaId}`} className="py-5 first:pt-0">
            <div className="flex flex-col justify-between gap-3 sm:flex-row sm:items-start">
              <div className="min-w-0"><Link href={`/water-bodies/${item.corpoHidricoId}/dashboard`} className="break-words font-heading font-bold text-primary hover:underline">{item.corpoHidricoNome}</Link>
                <p className="mt-1 text-xs text-text-secondary">{item.localizacao}</p>
                <p className="mt-2 text-sm">Coleta #{item.coletaId} · {formatMonitoringDate(item.dataHora)}</p>
                {item.responsavelNome && <p className="mt-1 text-xs text-text-secondary">Responsável: {item.responsavelNome}</p>}
              </div>
              <RiskBadge level={item.risco?.level} unavailable={item.predicaoId !== null && !item.risco} />
            </div>
            {item.predicaoId ? <p className="mt-2 text-xs text-text-secondary">Análise #{item.predicaoId}{item.analisadaEm ? ` · ${formatMonitoringDate(item.analisadaEm)}` : ""}</p> : <p className="mt-2 text-xs text-text-secondary">Coleta salva, ainda sem resultado de risco da IA.</p>}
            {!waterBodyId && !item.predicaoId && <Link href={`/add-analysis?coletaId=${item.coletaId}`} className="mt-2 inline-block text-sm font-semibold text-primary underline underline-offset-4">Concluir análise com IA</Link>}
            {!!item.risco?.reasons.length && <details className="mt-3 text-sm"><summary className="w-fit cursor-pointer font-semibold text-primary">Motivos da classificação</summary><ul className="mt-2 list-disc space-y-1 pl-5 text-text-secondary">{item.risco.reasons.map((reason, index) => <li key={index}>{reason}</li>)}</ul></details>}
          </li>)}</ul>}
        <nav aria-label="Paginação das análises" className="mt-4 flex flex-wrap items-center justify-between gap-3 border-t border-placeholder pt-4">
          <Button variant="outline" disabled={page === 1} onClick={() => setPage(value => value - 1)}><LuChevronLeft aria-hidden="true" />Anterior</Button>
          <span className="text-xs text-text-secondary">Página {page} de {totalPages}</span>
          <Button variant="outline" disabled={page >= totalPages} onClick={() => setPage(value => value + 1)}>Próxima<LuChevronRight aria-hidden="true" /></Button>
        </nav>
      </Card>}
    <p className="text-xs text-text-secondary">Risco estimado pela IA: 1 baixo, 2 moderado e 3 alto. O nível de risco não representa um IQA ou uma certificação de potabilidade.</p>
  </div>;
}
