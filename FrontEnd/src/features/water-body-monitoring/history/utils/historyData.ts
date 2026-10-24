import type { MonitoringColetaDTO } from "@/api/dtos/monitoringDTO";
import { collectionIqa, comparableAnalysisValue } from "../../analysis/utils/analysisData";
import { dashboardMetrics, formatMetric, formatMonitoringDate, getMeasurement } from "../../dashboard/utils/dashboardData";

export const historyIndicators = [
  { key: "iqa", label: "IQA medido", unit: "pontos" },
  ...dashboardMetrics,
] as const;

export type HistoryIndicator = typeof historyIndicators[number];

export function historyValue(collection: MonitoringColetaDTO, indicator: HistoryIndicator): number | null {
  return indicator.key === "iqa" ? collectionIqa(collection) : comparableAnalysisValue(collection, indicator);
}

export function historyMeasurement(collection: MonitoringColetaDTO, indicator: HistoryIndicator): string {
  if (indicator.key !== "iqa") return formatMetric(getMeasurement(collection, indicator));
  const value = collectionIqa(collection);
  return value === null ? "—" : `${value.toLocaleString("pt-BR", { maximumFractionDigits: 2 })} pontos`;
}

function csvCell(value: string): string {
  return `"${value.replace(/^[=+@-]/, "'$&").replaceAll('"', '""')}"`;
}

export function buildHistoryCsv(collections: MonitoringColetaDTO[]): string {
  const rows = [
    ["Coleta", "Data e hora (America/Sao_Paulo)", ...historyIndicators.map((item) => item.label)],
    ...collections.map((item) => [String(item.id), formatMonitoringDate(item.dataHora), ...historyIndicators.map((indicator) => historyMeasurement(item, indicator))]),
  ];
  return "\uFEFF" + rows.map((row) => row.map(csvCell).join(";")).join("\r\n");
}
