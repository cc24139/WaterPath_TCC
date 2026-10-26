"use client";

import { useState } from "react";
import { LuCalendarDays, LuChartNoAxesCombined, LuDroplet, LuInfo } from "react-icons/lu";
import type { MonitoringColetaDTO } from "@/api/dtos/monitoringDTO";
import { Card } from "@/components/ui/Card";
import { historyIndicators, historyMeasurement } from "../../history/utils/historyData";
import { formatMonitoringDate } from "../../dashboard/utils/dashboardData";
import { DashboardPanel } from "../../dashboard/components/DashboardPanel";
import { AnalysisIndicatorsChart } from "./AnalysisIndicatorsChart";
import { RiverConditionCard } from "./RiverConditionCard";
import { RiverInsightListCard } from "./RiverInsightListCard";
import { buildRiverOverview, type OverviewPeriod } from "../utils/riverOverview";

const periods: { value: OverviewPeriod; label: string }[] = [
  { value: 90, label: "90 dias" },
  { value: 365, label: "12 meses" },
];
const recentIndicators = [historyIndicators[0], historyIndicators[1], historyIndicators[3]];

const number = (value: number) => value.toLocaleString("pt-BR", { maximumFractionDigits: 2 });

export function RiverOverview({ collections, until, basePath }: {
  collections: MonitoringColetaDTO[];
  until: number | null;
  basePath: string;
}) {
  const [period, setPeriod] = useState<OverviewPeriod>(365);
  const overview = buildRiverOverview(collections, period, until ?? 0);
  const recent = overview.collections.slice(0, 3);
  const insights = overview.indicators.map(({ metric, count, firstMedian, lastMedian, groupSize }) => ({
    title: metric.label,
    description: overview.mixedContext
      ? "Comparação suspensa: as coletas incluem locais ou profundidades diferentes."
      : firstMedian === null || lastMedian === null
        ? `${count} ${count === 1 ? "medição válida" : "medições válidas"}. Para comparar, são necessárias 6 medições distribuídas por pelo menos 30 dias.`
        : `Mediana dos primeiros ${groupSize} registros: ${number(firstMedian)} ${metric.unit || "pH"}; dos últimos ${groupSize}: ${number(lastMedian)} ${metric.unit || "pH"}.`,
    colorClass: "text-primary",
    icon: <LuChartNoAxesCombined className="h-5 w-5" aria-hidden="true" />,
  }));

  return <div className="space-y-5">
    <div className="flex flex-col justify-between gap-3 sm:flex-row sm:items-end">
      <div>
        <h2 className="font-heading text-lg font-bold">Panorama do rio</h2>
        <p className="mt-1 max-w-2xl text-xs leading-relaxed text-text-secondary">Comparação descritiva das medições registradas. O panorama não é uma nova análise da IA nem uma previsão.</p>
      </div>
      <div role="group" aria-label="Período do panorama" className="flex flex-wrap gap-2">
        {periods.map((item) => <button key={item.value} type="button" aria-pressed={period === item.value}
          onClick={() => setPeriod(item.value)} className={`rounded-md border px-3 py-2 text-xs font-semibold transition-colors focus-visible:outline-2 focus-visible:outline-primary ${period === item.value ? "border-primary bg-primary text-white" : "border-placeholder bg-white text-text-secondary hover:border-primary hover:text-primary"}`}>
          {item.label}
        </button>)}
      </div>
    </div>

    <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3">
      <RiverConditionCard title="Coletas no período" value={String(overview.count)}
        description={overview.count ? `${overview.spanDays} dias entre a primeira e a última` : "Nenhuma coleta neste período"}
        colorClass="text-primary" icon={<LuCalendarDays className="h-6 w-6" aria-hidden="true" />} />
      <RiverConditionCard title="Último IQA medido" value={overview.latestIqa === null ? "Sem IQA" : `${number(overview.latestIqa)} / 100`}
        description={`${overview.iqaCount} ${overview.iqaCount === 1 ? "coleta com IQA" : "coletas com IQA"} no período`}
        colorClass="text-primary" icon={<LuDroplet className="h-6 w-6" aria-hidden="true" />} />
      <RiverConditionCard title="Indicadores comparáveis" value={`${overview.comparisonCount} de ${overview.indicators.length}`}
        description="Medianas do início e do fim dos registros"
        colorClass="text-primary" icon={<LuChartNoAxesCombined className="h-6 w-6" aria-hidden="true" />} />
    </div>

    {overview.count > 0 && (overview.mixedContext || overview.incompleteContext) && <Card className="flex items-start gap-3 border border-primary/10 !p-4 text-xs leading-relaxed text-text-secondary">
      <LuInfo className="mt-0.5 h-4 w-4 shrink-0 text-primary" aria-hidden="true" />
      <p>{overview.mixedContext
        ? "Há coletas em locais ou profundidades diferentes. A comparação de medianas foi suspensa; o gráfico apenas mostra os registros do rio."
        : "Nem todas as coletas têm localização e profundidade registradas. A comparação descreve os registros do rio, sem representar um ponto fixo."}</p>
    </Card>}

    <div className="grid items-stretch gap-4 xl:grid-cols-[minmax(0,1.7fr)_minmax(280px,1fr)]">
      <AnalysisIndicatorsChart collections={overview.collections} until={until} days={period} title="Evolução dos indicadores no período" />
      <RiverInsightListCard title="Leitura do período" items={insights} actionLabel="Explorar medições" actionHref={`${basePath}/history`} />
    </div>

    <DashboardPanel title="Coletas recentes" action={{ label: "Ver histórico completo", href: `${basePath}/history` }}>
      {recent.length ? <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-3">{recent.map((collection) => <article key={collection.id} className="min-w-0 rounded-xl border border-placeholder/70 p-4">
        <h3 className="font-heading text-sm font-bold">{formatMonitoringDate(collection.dataHora)}</h3>
        <p className="mt-0.5 text-xs text-text-secondary">Coleta #{collection.id}</p>
        <dl className="mt-3 grid gap-2 border-t border-placeholder/60 pt-3">
          {recentIndicators.map((indicator) => <div key={indicator.key} className="flex min-w-0 justify-between gap-2 text-xs">
            <dt className="text-text-secondary">{indicator.label}</dt>
            <dd className="text-right font-semibold">{historyMeasurement(collection, indicator)}</dd>
          </div>)}
        </dl>
      </article>)}</div> : <p role="status" className="py-8 text-center text-sm text-text-secondary">Nenhuma coleta registrada no período selecionado.</p>}
    </DashboardPanel>

    <p className="text-xs leading-relaxed text-text-secondary">As medianas comparam a primeira e a última metade das medições válidas de cada indicador, com pelo menos 6 valores distribuídos por 30 dias. O gráfico usa escalas relativas separadas; a altura entre cores não compara concentrações. Valores sem unidade esperada ou abaixo do limite de detecção não entram na comparação.</p>
  </div>;
}
