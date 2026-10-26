import type { MonitoringColetaDTO } from "@/api/dtos/monitoringDTO";
import { dashboardMetrics, getMeasurement, measurementValue, type DashboardMetric } from "../../dashboard/utils/dashboardData.ts";

export const analysisIndicators = [dashboardMetrics[0], dashboardMetrics[2], dashboardMetrics[1]] as const;

export function collectionIqa(collection: MonitoringColetaDTO | undefined): number | null {
  const matches = collection?.medicoes.filter((measurement) => measurement.codigoMedicao === 17
    || measurement.codigoMedicao === "17" || measurement.codigoMedicao === "Iqa") ?? [];
  if (matches.length !== 1) return null;
  const value = matches[0].valor;
  return !matches[0].censurado && matches[0].unidade === "pontos"
    && typeof value === "number" && Number.isFinite(value) && value >= 0 && value <= 100 ? value : null;
}

export function comparableAnalysisValue(collection: MonitoringColetaDTO, metric: DashboardMetric): number | null {
  const measurement = getMeasurement(collection, metric);
  const value = measurementValue(measurement);
  if (value === null || (value < 0 && metric.key !== "temperature") || (metric.key === "ph" && value > 14)) return null;
  const unit = measurement?.unidade.replace(/[μu]/g, "µ");
  const expected = metric.key === "ph" ? "pH" : metric.unit;
  return unit === expected ? value : null;
}
