import type { MonitoringColetaDTO } from "@/api/dtos/monitoringDTO";
import { DashboardPanel } from "../../dashboard/components/DashboardPanel";
import { dashboardMetrics, formatMonitoringDate } from "../../dashboard/utils/dashboardData";
import { comparableAnalysisValue } from "../utils/analysisData";

const indicators = [
  { metric: dashboardMetrics[0], color: "#ef4444" },
  { metric: dashboardMetrics[2], color: "#22a6bf" },
  { metric: dashboardMetrics[1], color: "#9381ff" },
] as const;

export function AnalysisIndicatorsChart({ collections, until }: { collections: MonitoringColetaDTO[]; until: number | null }) {
  const end = until ?? 0;
  const start = end - 90 * 86400000;
  const chronological = collections.filter((item) => {
    const time = Date.parse(item.dataHora);
    return Number.isFinite(time) && time >= start && time <= end;
  }).sort((a, b) => Date.parse(a.dataHora) - Date.parse(b.dataHora) || a.id - b.id);
  const x = (time: number) => 45 + (time - start) / (end - start) * 770;
  const series = indicators.map(({ metric, color }) => {
    const points = chronological.map((collection) => ({ id: collection.id, time: Date.parse(collection.dataHora), value: comparableAnalysisValue(collection, metric) }));
    const valid = points.filter((point) => point.value !== null);
    return { metric, color, points, valid,
      maximum: metric.key === "ph" ? 14 : Math.max(1, ...valid.map((point) => point.value!)) * 1.1,
      unit: metric.key === "ph" ? "pH" : metric.unit };
  });
  const hasValues = series.some((item) => item.valid.length > 0);

  return <DashboardPanel title="Evolução dos principais indicadores (últimos 90 dias)" className="h-full">
    <div className="space-y-3">
      <div className="flex flex-wrap gap-x-5 gap-y-2 text-xs">
        {series.map(({ metric, color, valid, unit }) => <div key={metric.key} className="flex items-center gap-2">
          <span className="h-2.5 w-2.5 shrink-0 rounded-full" style={{ backgroundColor: color }} />
          <span className="font-semibold text-text-primary">{metric.label}</span>
          <span className="text-text-secondary">{valid.length ? `${valid.at(-1)!.value!.toLocaleString("pt-BR", { maximumFractionDigits: 2 })} ${unit}` : "sem dados"}</span>
        </div>)}
      </div>
      {hasValues && <p className="text-[11px] text-text-secondary">Eixo vertical: posição relativa (0–100) na escala de cada indicador.</p>}
      {hasValues ? <div className="overflow-x-auto"><svg viewBox="0 0 860 240" role="img" aria-label="Evolução relativa de pH, oxigênio dissolvido e condutividade elétrica nos últimos 90 dias. Cada linha tem uma escala própria; consulte os valores absolutos na legenda e nos pontos." className="h-52 w-full min-w-[560px] sm:h-60">
        {[0, 25, 50, 75, 100].map((tick) => { const y = 205 - tick / 100 * 165; return <g key={tick}>
          <line x1="45" x2="815" y1={y} y2={y} stroke="var(--color-placeholder)" strokeDasharray={tick === 0 ? undefined : "3 4"} />
          <text x="38" y={y + 4} textAnchor="end" fontSize="11" fill="var(--color-text-secondary)">{tick}</text>
        </g>; })}
        {series.map(({ metric, color, points, valid, maximum, unit }) => {
          const y = (value: number) => 205 - value / maximum * 165;
          return <g key={metric.key}>
            {points.map((point, index) => { const previous = points[index - 1];
              return point.value !== null && previous?.value !== null && previous?.value !== undefined
                ? <line key={`line-${point.id}`} x1={x(previous.time)} x2={x(point.time)} y1={y(previous.value)} y2={y(point.value)} stroke={color} strokeWidth="2.5" /> : null;
            })}
            {valid.map((point) => <circle key={point.id} cx={x(point.time)} cy={y(point.value!)} r="3.5" fill="white" stroke={color} strokeWidth="2">
              <title>{metric.label} · {formatMonitoringDate(point.time)}: {point.value} {unit}</title>
            </circle>)}
          </g>;
        })}
        <text x="45" y="232" fontSize="11" fill="var(--color-text-secondary)">{formatMonitoringDate(start)}</text>
        <text x="815" y="232" textAnchor="end" fontSize="11" fill="var(--color-text-secondary)">{formatMonitoringDate(end)}</text>
      </svg></div> : <p role="status" className="flex min-h-52 items-center justify-center rounded-lg bg-background/50 p-6 text-center text-sm text-text-secondary">Não há medições comparáveis nos últimos 90 dias.</p>}
      {hasValues && <p className="text-xs leading-relaxed text-text-secondary">Altura relativa de cada linha (0–100) na sua própria escala: {series.map(({ metric, maximum, unit }) => `${metric.label} 0–${maximum.toLocaleString("pt-BR", { maximumFractionDigits: 1 })} ${unit}`).join(" · ")}. Compare a tendência de cada cor; alturas entre indicadores não representam a mesma concentração. Coletas sem valor válido interrompem a linha.</p>}
    </div>
  </DashboardPanel>;
}
