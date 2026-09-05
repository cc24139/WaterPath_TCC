import type { RiverMeasurement } from "../types/river";

export type MeasurementMetric = "ph" | "turbidity" | "dissolvedOxygen";

export function measurementChart(measurements: RiverMeasurement[], metric: MeasurementMetric) {
  const largest = measurements.reduce((max, point) => Math.max(max, point[metric] ?? 0), 1);
  const maximum = metric === "ph" ? 14 : Math.min(largest, Number.MAX_VALUE / 1.1) * 1.1;
  const start = measurements[0]?.timestamp ?? 0;
  const end = measurements.at(-1)?.timestamp ?? start;
  const points = measurements.map((point) => ({
    ...point,
    value: point[metric],
    x: start === end ? 165 : 45 + ((point.timestamp - start) / (end - start)) * 240,
    y: point[metric] === null ? null : 130 - (point[metric] / maximum) * 105,
  }));
  const segments = points.flatMap((point, index) => {
    const previous = points[index - 1];
    return previous && previous.y !== null && point.y !== null && previous.timestamp < point.timestamp
      ? [{ from: previous, to: point }]
      : [];
  });
  return { points, segments, start, end, maximum };
}
