import type { MedicaoDTO, MonitoringColetaDTO } from "../../../../api/dtos/monitoringDTO";

export const dashboardMetrics = [
  { key: "ph", code: 1, name: "Ph", label: "pH", unit: "" },
  { key: "conductivity", code: 4, name: "CondutividadeEletrica", label: "Condutividade elétrica", unit: "µS/cm" },
  { key: "oxygen", code: 2, name: "OxigenioDissolvido", label: "Oxigênio dissolvido", unit: "mg/L" },
  { key: "temperature", code: 0, name: "Temperatura", label: "Temperatura", unit: "°C" },
] as const;
export type DashboardMetric = typeof dashboardMetrics[number];
export const dashboardPeriods = [
  { value: "7", label: "7 dias" }, { value: "30", label: "30 dias" },
  { value: "90", label: "90 dias" }, { value: "365", label: "12 meses" },
] as const;

export function getMeasurement(collection: MonitoringColetaDTO | undefined, metric: DashboardMetric): MedicaoDTO | undefined {
  return collection?.medicoes.find((item) => item.codigoMedicao === metric.code || item.codigoMedicao === String(metric.code) || String(item.codigoMedicao).toLowerCase() === metric.name.toLowerCase());
}

export function measurementValue(measurement: MedicaoDTO | undefined): number | null {
  return measurement && !measurement.censurado && typeof measurement.valor === "number" && Number.isFinite(measurement.valor) ? measurement.valor : null;
}

export function formatMetric(measurement: MedicaoDTO | undefined, withUnit = true): string {
  if (!measurement) return "—";
  const value = measurement.censurado ? measurement.limite : measurement.valor;
  if (typeof value !== "number" || !Number.isFinite(value)) return "—";
  return `${measurement.censurado ? "< " : ""}${value.toLocaleString("pt-BR", { maximumFractionDigits: 2 })}${withUnit && measurement.unidade ? ` ${measurement.unidade}` : ""}`;
}

export function formatMonitoringDate(value: string | number): string {
  return new Date(value).toLocaleString("pt-BR", { timeZone: "America/Sao_Paulo", dateStyle: "short", timeStyle: "short" });
}

export function readDashboardCollections(payload: unknown, waterBodyId: number): MonitoringColetaDTO[] {
  if (!Array.isArray(payload)) throw new Error("Formato de coletas inválido");
  const ids = new Set<number>();
  for (const item of payload) {
    if (!item || !Number.isInteger(item.id) || item.id <= 0 || ids.has(item.id) || item.corpoHidricoId !== waterBodyId || typeof item.dataHora !== "string" || !Number.isFinite(Date.parse(item.dataHora)) || !Array.isArray(item.medicoes)) {
      throw new Error("Coleta inválida");
    }
    ids.add(item.id);
    for (const measurement of item.medicoes) {
      if (!measurement || !["number", "string"].includes(typeof measurement.codigoMedicao) || typeof measurement.unidade !== "string" || typeof measurement.censurado !== "boolean" || (measurement.valor != null && (typeof measurement.valor !== "number" || !Number.isFinite(measurement.valor))) || (measurement.limite != null && (typeof measurement.limite !== "number" || !Number.isFinite(measurement.limite)))) {
        throw new Error("Medição inválida");
      }
    }
  }
  return [...payload].sort((a, b) => Date.parse(b.dataHora) - Date.parse(a.dataHora) || b.id - a.id);
}

export function filterCollections(collections: MonitoringColetaDTO[], days: number, now: number): MonitoringColetaDTO[] {
  const from = now - days * 86400000;
  return collections.filter((item) => { const date = Date.parse(item.dataHora); return date >= from && date <= now; });
}

export function buildMeasurementsCsv(collections: MonitoringColetaDTO[]): string {
  const cell = (value: string) => `"${value.replace(/^[=+@-]/, "'$&").replaceAll('"', '""')}"`;
  const rows = [["Coleta", "Data e hora (America/Sao_Paulo)", ...dashboardMetrics.map((metric) => metric.label)],
    ...collections.map((item) => [String(item.id), formatMonitoringDate(item.dataHora), ...dashboardMetrics.map((metric) => formatMetric(getMeasurement(item, metric)))])];
  return "\uFEFF" + rows.map((row) => row.map(cell).join(";")).join("\r\n");
}
