import type { MonitoringColetaDTO } from "@/api/dtos/monitoringDTO";
import { filterCollections, type DashboardMetric } from "../../dashboard/utils/dashboardData.ts";
import { analysisIndicators, collectionIqa, comparableAnalysisValue } from "./analysisData.ts";

const day = 86_400_000;
const minimumReadings = 6;
const minimumSpanDays = 30;

export type OverviewPeriod = 90 | 365;

export interface IndicatorComparison {
  metric: DashboardMetric;
  count: number;
  firstMedian: number | null;
  lastMedian: number | null;
  groupSize: number;
}

function median(values: number[]): number {
  const sorted = [...values].sort((a, b) => a - b);
  const middle = Math.floor(sorted.length / 2);
  return sorted.length % 2 ? sorted[middle] : (sorted[middle - 1] + sorted[middle]) / 2;
}

export function buildRiverOverview(collections: MonitoringColetaDTO[], period: OverviewPeriod, until: number) {
  const chronological = filterCollections(collections, period, until)
    .sort((a, b) => Date.parse(a.dataHora) - Date.parse(b.dataHora) || a.id - b.id);
  const inPeriod = [...chronological].reverse();
  const firstAt = chronological.length ? Date.parse(chronological[0].dataHora) : null;
  const lastAt = chronological.length ? Date.parse(chronological.at(-1)!.dataHora) : null;
  const spanDays = firstAt === null || lastAt === null ? 0 : Math.floor((lastAt - firstAt) / day);
  const iqaCollections = inPeriod.filter((collection) => collectionIqa(collection) !== null);

  const locations = inPeriod.filter((item) => typeof item.latitude === "number" && Number.isFinite(item.latitude)
    && typeof item.longitude === "number" && Number.isFinite(item.longitude));
  const depths = inPeriod.filter((item) => typeof item.profundidadeMetros === "number" && Number.isFinite(item.profundidadeMetros));
  const mixedLocation = locations.length > 1 && locations.some((item) =>
    Math.abs(item.latitude! - locations[0].latitude!) > 0.001 || Math.abs(item.longitude! - locations[0].longitude!) > 0.001);
  const mixedDepth = depths.length > 1 && depths.some((item) => Math.abs(item.profundidadeMetros! - depths[0].profundidadeMetros!) > 0.5);
  const mixedContext = mixedLocation || mixedDepth;
  const incompleteContext = locations.length !== inPeriod.length || depths.length !== inPeriod.length;

  const indicators: IndicatorComparison[] = analysisIndicators.map((metric) => {
    const readings = chronological.map((collection) => ({
      at: Date.parse(collection.dataHora), value: comparableAnalysisValue(collection, metric),
    })).filter((reading): reading is { at: number; value: number } => reading.value !== null);
    const groupSize = Math.floor(readings.length / 2);
    const enoughTime = readings.length > 0 && readings.at(-1)!.at - readings[0].at >= minimumSpanDays * day;
    if (mixedContext || readings.length < minimumReadings || !enoughTime) {
      return { metric, count: readings.length, firstMedian: null, lastMedian: null, groupSize: 0 };
    }
    return {
      metric, count: readings.length, groupSize,
      firstMedian: median(readings.slice(0, groupSize).map((item) => item.value)),
      lastMedian: median(readings.slice(-groupSize).map((item) => item.value)),
    };
  });

  return {
    collections: inPeriod,
    count: inPeriod.length,
    firstAt,
    lastAt,
    spanDays,
    iqaCount: iqaCollections.length,
    latestIqa: collectionIqa(iqaCollections[0]),
    mixedContext,
    incompleteContext,
    indicators,
    comparisonCount: indicators.filter((item) => item.firstMedian !== null).length,
  };
}
