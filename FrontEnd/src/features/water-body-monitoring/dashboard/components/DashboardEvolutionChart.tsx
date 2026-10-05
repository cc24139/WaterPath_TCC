"use client";

import { useId, useState } from "react";
import type { MonitoringColetaDTO } from "@/api/dtos/monitoringDTO";
import { dashboardMetrics, dashboardPeriods, formatMonitoringDate, getMeasurement, measurementValue } from "../utils/dashboardData";
import { DashboardPanel } from "./DashboardPanel";

export function DashboardEvolutionChart({ collections, period, onPeriodChange }: {
  collections: MonitoringColetaDTO[]; period: string; onPeriodChange: (value: string) => void;
}) {
  const [metricKey, setMetricKey] = useState("iqa");
  const selectId = useId();
  const metric = dashboardMetrics.find((item) => item.key === metricKey);
  const chronological = [...collections].reverse();
  const units = new Set(metric ? collections.flatMap((item) => { const measurement = getMeasurement(item, metric); return measurementValue(measurement) !== null ? [measurement!.unidade] : []; }) : []);
  const points = metric ? chronological.map((item) => ({ id: item.id, time: Date.parse(item.dataHora), value: measurementValue(getMeasurement(item, metric)) })) : [];
  const valid = points.filter((point) => point.value !== null);
  const maximum = metric?.key === "ph" ? Math.max(14, ...valid.map((point) => point.value!)) : Math.max(1, ...valid.map((point) => point.value!)) * 1.1;
  const start = points[0]?.time ?? 0, end = points.at(-1)?.time ?? start;
  const x = (time: number) => end === start ? 430 : 55 + (time - start) / (end - start) * 750;
  const y = (value: number) => 170 - value / maximum * 140;
  return (
    <DashboardPanel title={metric ? `Evolução de ${metric.label}` : "Evolução do IQA"}>
      <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
        <div><label htmlFor={selectId} className="sr-only">Indicador do gráfico</label><select id={selectId} value={metricKey} onChange={(event) => setMetricKey(event.target.value)} className="rounded-md border border-placeholder bg-white p-2 text-xs focus-visible:outline-primary">
          <option value="iqa">IQA</option>{dashboardMetrics.map((item) => <option key={item.key} value={item.key}>{item.label}</option>)}
        </select></div>
        <div aria-label="Período do gráfico" className="flex flex-wrap gap-1">{dashboardPeriods.map((item) => <button key={item.value} type="button" aria-pressed={period === item.value} onClick={() => onPeriodChange(item.value)} className={`rounded-md border px-3 py-2 text-xs focus-visible:outline-primary ${period === item.value ? "border-primary bg-primary text-white" : "border-placeholder text-text-secondary hover:bg-primary/5"}`}>{item.label}</button>)}</div>
      </div>
      {!metric || valid.length === 0 || units.size > 1 ? <div role="status" className="flex min-h-48 items-center justify-center rounded-lg bg-background/50 p-6 text-center text-sm text-text-secondary">
        {!metric ? "O histórico de IQA ainda não está disponível. Selecione um parâmetro para visualizar as medições." : units.size > 1 ? "As medições têm unidades diferentes e não podem ser comparadas neste gráfico." : "Não há medições numéricas disponíveis para este indicador no período."}
      </div> : <>
        <svg viewBox="0 0 860 215" role="img" aria-label={`Evolução de ${metric.label}, em ${[...units][0] || "escala de pH"}. ${valid.length} medições. Valores na exportação CSV.`} className="h-52 w-full">
          {[0, maximum / 2, maximum].map((tick) => <g key={tick}><line x1="55" x2="805" y1={y(tick)} y2={y(tick)} stroke="var(--color-placeholder)" /><text x="45" y={y(tick) + 4} textAnchor="end" fontSize="12" fill="var(--color-text-secondary)">{tick.toLocaleString("pt-BR", { maximumFractionDigits: 1 })}</text></g>)}
          {points.map((point, index) => {
            const previous = points[index - 1];
            return point.value !== null && previous?.value != null ? <line key={`line-${point.id}`} x1={x(previous.time)} x2={x(point.time)} y1={y(previous.value)} y2={y(point.value)} stroke="#9381ff" strokeWidth="2.5" /> : null;
          })}
          {valid.map((point) => <circle key={point.id} cx={x(point.time)} cy={y(point.value!)} r="4" fill="#9381ff"><title>{formatMonitoringDate(point.time)}: {point.value} {[...units][0]}</title></circle>)}
          <text x="55" y="200" fontSize="12" fill="var(--color-text-secondary)">{formatMonitoringDate(start)}</text>{end !== start && <text x="805" y="200" textAnchor="end" fontSize="12" fill="var(--color-text-secondary)">{formatMonitoringDate(end)}</text>}
        </svg>
        <p className="text-xs text-text-secondary">{[...units][0] || "Escala de pH"} · {valid.length === 1 ? "Uma medição disponível; ainda não há tendência." : "Pontos nas datas das coletas, em horário de Brasília."} Medições censuradas não são usadas como valores exatos.</p>
      </>}
    </DashboardPanel>
  );
}
