"use client";

import { useId, useState } from "react";
import type { RiverMeasurement } from "../types/river";
import { formatCollectionDate, formatMeasurement } from "../utils/riverData";
import { measurementChart } from "../utils/measurementChart";

const metrics = {
  ph: { label: "pH" },
  turbidity: { label: "Turbidez" },
  dissolvedOxygen: { label: "Oxigênio dissolvido" },
} as const;
type Metric = keyof typeof metrics;

export function RiverMeasurementsChart({ measurements }: { measurements: RiverMeasurement[] }) {
  const [metric, setMetric] = useState<Metric>("ph");
  const selectId = useId();
  const settings = metrics[metric];
  const { points, segments, maximum, start, end } = measurementChart(measurements, metric);
  const validPoints = points.filter((point) => point.value !== null);
  const getY = (value: number) => 130 - (value / maximum) * 105;
  const ticks = [0, maximum / 2, maximum];

  return (
    <div className="min-w-0">
      <label htmlFor={selectId} className="block font-heading text-sm font-bold text-text-primary">Histórico das coletas</label>
      <select id={selectId} value={metric} onChange={(event) => setMetric(event.target.value as Metric)} className="mt-2 w-full rounded-md border border-placeholder bg-white px-2 py-2 text-xs text-text-primary focus-visible:outline-primary">
        {Object.entries(metrics).map(([key, option]) => <option key={key} value={key}>{option.label}</option>)}
      </select>
      {validPoints.length === 0 ? (
        <p role="status" className="flex min-h-36 items-center justify-center px-3 text-center text-sm text-text-secondary">Sem medições válidas de {settings.label} para exibir.</p>
      ) : (
        <>
          <svg viewBox="0 0 330 175" role="img" aria-label={`Histórico de ${settings.label}: ${validPoints.length} medição(ões). Datas e valores disponíveis na tabela abaixo.`} className="mt-3 h-40 w-full">
            {ticks.map((tick) => (
              <g key={tick}>
                <line x1="45" x2="285" y1={getY(tick)} y2={getY(tick)} stroke="var(--color-placeholder)" />
                <text x="39" y={getY(tick) + 4} textAnchor="end" fontSize="10" fill="var(--color-text-secondary)">{Number(tick.toPrecision(3)).toLocaleString("pt-BR")}</text>
              </g>
            ))}
            {segments.map(({ from, to }) => <line key={`${from.id}-${to.id}`} x1={from.x} y1={from.y!} x2={to.x} y2={to.y!} stroke="var(--color-primary)" strokeWidth="2" />)}
            {validPoints.map((point) => (
              <circle key={point.id} cx={point.x} cy={point.y!} r="4" fill="var(--color-primary)">
                <title>{formatCollectionDate(point.timestamp)}: {formatMeasurement(point.value)}</title>
              </circle>
            ))}
            <text x={start === end ? 165 : 45} y="155" textAnchor={start === end ? "middle" : "start"} fontSize="10" fill="var(--color-text-secondary)">{new Date(start).toLocaleDateString("pt-BR", { timeZone: "UTC" })}</text>
            {end !== start && <text x="285" y="155" textAnchor="end" fontSize="10" fill="var(--color-text-secondary)">{new Date(end).toLocaleDateString("pt-BR", { timeZone: "UTC" })}</text>}
          </svg>
          <p className="text-xs text-text-secondary">{validPoints.length === 1 ? "Uma coleta disponível; ainda não há uma tendência." : "Pontos nas datas das coletas; linhas apenas conectam medições consecutivas disponíveis."} Horários em UTC.</p>
        </>
      )}
      {measurements.length > 0 && (
        <details className="mt-3 text-xs text-text-secondary">
          <summary className="cursor-pointer rounded font-semibold text-primary focus-visible:outline-primary">Ver datas e valores</summary>
          <div className="mt-2 max-h-48 overflow-auto">
            <table className="w-full text-left">
              <caption className="sr-only">Medições de {settings.label}, em ordem cronológica, horários em UTC</caption>
              <thead><tr><th scope="col" className="py-2 pr-3">Data (UTC)</th><th scope="col" className="py-2">{settings.label}</th></tr></thead>
              <tbody>{measurements.map((point) => <tr key={point.id} className="border-t border-placeholder/60"><td className="py-2 pr-3">{formatCollectionDate(point.timestamp)}</td><td className="py-2">{formatMeasurement(point[metric])}</td></tr>)}</tbody>
            </table>
          </div>
        </details>
      )}
    </div>
  );
}
