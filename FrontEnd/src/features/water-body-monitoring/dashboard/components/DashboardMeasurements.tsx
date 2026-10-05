import { LuDroplet, LuThermometer, LuWaves, LuWind } from "react-icons/lu";
import type { MonitoringColetaDTO } from "@/api/dtos/monitoringDTO";
import { dashboardMetrics, formatMetric, formatMonitoringDate, getMeasurement } from "../utils/dashboardData";
import { DashboardPanel } from "./DashboardPanel";

const icons = [LuDroplet, LuWaves, LuWind, LuThermometer];

export function CurrentParametersCard({ latest, basePath }: { latest?: MonitoringColetaDTO; basePath: string }) {
  return (
    <DashboardPanel title="Parâmetros atuais" action={{ label: "Ver análise completa", href: `${basePath}/analysis` }}>
      <div className="grid grid-cols-2 gap-y-5 sm:grid-cols-4">
        {dashboardMetrics.map((metric, index) => {
          const Icon = icons[index];
          const measurement = getMeasurement(latest, metric);
          return <div key={metric.key} className="flex min-w-0 flex-col items-center gap-2 border-placeholder/60 px-2 text-center sm:border-r sm:last:border-0">
            <Icon aria-hidden="true" className="h-6 w-6 text-primary" />
            <h3 className="min-h-8 text-xs font-medium">{metric.label}</h3>
            <strong className="break-words text-sm">{formatMetric(measurement)}</strong>
            <span className="text-[10px] text-text-secondary">{measurement ? measurement.censurado ? "Abaixo do limite de detecção" : "Medido" : "Não medido"}</span>
          </div>;
        })}
      </div>
      <p className="mt-4 text-xs text-text-secondary">{latest ? `Coleta de ${formatMonitoringDate(latest.dataHora)} (Brasília).` : "Nenhuma coleta registrada."}</p>
    </DashboardPanel>
  );
}

export function RecentMeasurementsTable({ collections, basePath }: { collections: MonitoringColetaDTO[]; basePath: string }) {
  return (
    <DashboardPanel title="Histórico de medições" action={{ label: "Ver histórico completo", href: `${basePath}/history` }}>
      {collections.length === 0 ? <p role="status" className="py-8 text-center text-sm text-text-secondary">Nenhuma coleta no período selecionado.</p> : <div className="overflow-x-auto">
        <table className="w-full min-w-[650px] text-left text-xs">
          <caption className="sr-only">Até cinco coletas mais recentes do período. Horários de Brasília. IQA e classificação não disponíveis.</caption>
          <thead><tr className="border-b border-placeholder text-text-secondary">
            <th scope="col" className="pb-3 pr-4">Data e hora</th><th scope="col" className="pb-3 pr-4">IQA</th>
            {dashboardMetrics.map((metric) => <th key={metric.key} scope="col" className="pb-3 pr-4">{metric.label}</th>)}
            <th scope="col" className="pb-3">Status</th>
          </tr></thead>
          <tbody>{collections.slice(0, 5).map((collection) => <tr key={collection.id} className="border-b border-placeholder/50 last:border-0">
            <td className="whitespace-nowrap py-3 pr-4">{formatMonitoringDate(collection.dataHora)}</td><td className="py-3 pr-4">—</td>
            {dashboardMetrics.map((metric) => <td key={metric.key} className="whitespace-nowrap py-3 pr-4">{formatMetric(getMeasurement(collection, metric))}</td>)}
            <td className="py-3 text-text-secondary">Sem classificação</td>
          </tr>)}</tbody>
        </table>
      </div>}
      <p className="mt-3 text-[11px] text-text-secondary">Horários de Brasília. “—” indica dado indisponível; “&lt;” indica medição abaixo do limite de detecção.</p>
    </DashboardPanel>
  );
}
