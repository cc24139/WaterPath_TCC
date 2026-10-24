import type { MonitoringColetaDTO } from "@/api/dtos/monitoringDTO";
import { formatMonitoringDate } from "../../dashboard/utils/dashboardData";
import { historyValue, type HistoryIndicator } from "../utils/historyData";

export function HistoryChart({ collections, indicator }: { collections: MonitoringColetaDTO[]; indicator: HistoryIndicator }) {
  const chronological = [...collections].reverse();
  const values = chronological.map((collection) => ({
    collection,
    time: Date.parse(collection.dataHora),
    value: historyValue(collection, indicator),
  }));
  const valid = values.filter((point): point is typeof point & { value: number } => point.value !== null);
  if (!valid.length) return <div role="status" className="flex min-h-60 flex-col items-center justify-center rounded-xl bg-background/50 px-5 text-center">
    <p className="font-heading text-sm font-semibold">Sem valores de {indicator.label} neste período</p>
    <p className="mt-1 text-xs text-text-secondary">Escolha outro indicador ou amplie o período para ver a evolução.</p>
  </div>;

  const minimum = Math.min(...valid.map((point) => point.value));
  const maximum = Math.max(...valid.map((point) => point.value));
  const padding = Math.max((maximum - minimum) * 0.18, indicator.key === "iqa" ? 4 : Math.max(Math.abs(maximum), 1) * 0.08);
  const low = indicator.key === "iqa" ? Math.max(0, minimum - padding) : minimum - padding;
  const high = indicator.key === "iqa" ? Math.min(100, maximum + padding) : maximum + padding;
  const span = Math.max(high - low, 0.001);
  const start = valid[0].time;
  const end = valid.at(-1)!.time;
  const x = (time: number) => 56 + (end === start ? 0.5 : (time - start) / (end - start)) * 744;
  const y = (value: number) => 214 - (value - low) / span * 172;
  const segments: typeof valid[] = [];
  let current: typeof valid = [];
  for (const point of values) {
    if (point.value === null) {
      if (current.length) segments.push(current);
      current = [];
    } else current.push(point as typeof valid[number]);
  }
  if (current.length) segments.push(current);
  const formatTick = (value: number) => value.toLocaleString("pt-BR", { maximumFractionDigits: 2 });

  return <div className="overflow-x-auto">
    <svg viewBox="0 0 856 270" role="img" aria-label={`Evolução de ${indicator.label} em ${valid.length} coletas. Os valores exatos estão na tabela abaixo.`} className="h-64 w-full min-w-[540px]">
      <defs><linearGradient id="history-chart-fill" x1="0" x2="0" y1="0" y2="1"><stop offset="0%" stopColor="#22a6bf" stopOpacity="0.17" /><stop offset="100%" stopColor="#22a6bf" stopOpacity="0" /></linearGradient></defs>
      {[0, 1, 2, 3, 4].map((index) => {
        const tick = low + (high - low) * (4 - index) / 4;
        const top = y(tick);
        return <g key={index}><line x1="56" x2="800" y1={top} y2={top} stroke="var(--color-placeholder)" strokeDasharray={index === 4 ? undefined : "3 5"} />
          <text x="46" y={top + 4} textAnchor="end" fontSize="11" fill="var(--color-text-secondary)">{formatTick(tick)}</text></g>;
      })}
      {segments.map((segment, index) => <g key={index}>
        {segment.length > 1 && <path d={`M ${x(segment[0].time)} 214 L ${segment.map((point) => `${x(point.time)} ${y(point.value)}`).join(" L ")} L ${x(segment.at(-1)!.time)} 214 Z`} fill="url(#history-chart-fill)" />}
        {segment.length > 1 && <path d={`M ${segment.map((point) => `${x(point.time)} ${y(point.value)}`).join(" L ")}`} fill="none" stroke="#17a6bf" strokeWidth="3" strokeLinecap="round" strokeLinejoin="round" />}
      </g>)}
      {valid.length <= 50 && valid.map((point) => <circle key={point.collection.id} cx={x(point.time)} cy={y(point.value)} r="3.5" fill="white" stroke="#17a6bf" strokeWidth="2"><title>{formatMonitoringDate(point.collection.dataHora)} · {formatTick(point.value)} {indicator.unit} · Coleta #{point.collection.id}</title></circle>)}
      <text x="56" y="252" fontSize="11" fill="var(--color-text-secondary)">{formatMonitoringDate(start)}</text>
      {end !== start && <text x="800" y="252" textAnchor="end" fontSize="11" fill="var(--color-text-secondary)">{formatMonitoringDate(end)}</text>}
    </svg>
  </div>;
}
