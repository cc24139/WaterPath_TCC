import type { River, RiverMeasurement, RiverStatus } from "../types/river";

type JsonRecord = Record<string, unknown>;

function record(value: unknown): JsonRecord {
  return value !== null && typeof value === "object" && !Array.isArray(value)
    ? value as JsonRecord
    : {};
}

function field(value: unknown, name: string): unknown {
  const data = record(value);
  return data[name] ?? data[name[0].toUpperCase() + name.slice(1)];
}

export function readNumber(value: unknown): number | null {
  if (typeof value === "string") {
    const text = value.trim();
    if (!/^[+-]?\d+(?:[.,]\d+)?(?:e[+-]?\d+)?$/i.test(text)) return null;
    value = Number(text.replace(",", "."));
  }
  return typeof value === "number" && Number.isFinite(value) ? value : null;
}

function id(value: unknown): string | null {
  const number = readNumber(value);
  return number !== null && Number.isSafeInteger(number) && number > 0 ? String(number) : null;
}

function bodyId(value: unknown): string | null {
  return id(field(value, "corpoHidricoId")) ?? id(field(field(value, "corpoHidrico"), "id"));
}

function text(value: unknown): string | null {
  return typeof value === "string" && value.trim() ? value.trim() : null;
}

function metric(value: unknown, max = Infinity): number | null {
  const number = readNumber(value);
  return number !== null && number >= 0 && number <= max ? number : null;
}

export function readTimestamp(value: unknown): number | null {
  if (typeof value !== "string") return null;
  const match = /^(\d{4})-(\d{2})-(\d{2})(?:T\d{2}:\d{2}:\d{2}(?:\.\d+)?(?:Z|[+-]\d{2}:\d{2})?)?$/.exec(value);
  if (!match || Number(match[1]) < 1900) return null;
  const calendarDate = new Date(Date.UTC(Number(match[1]), Number(match[2]) - 1, Number(match[3])));
  if (calendarDate.toISOString().slice(0, 10) !== value.slice(0, 10)) return null;
  // .NET can serialize an unspecified DateTime without an offset. Keep its
  // calendar date stable across browsers by interpreting that form as UTC.
  const iso = value.length === 10 ? `${value}T00:00:00Z`
    : /(?:Z|[+-]\d{2}:\d{2})$/.test(value) ? value : `${value}Z`;
  const timestamp = Date.parse(iso);
  return Number.isFinite(timestamp) ? timestamp : null;
}

// Existing visual bands in this project, not a new regulatory classification.
export function getRiverStatus(iqa: number | null): RiverStatus {
  if (iqa === null) return "Sem classificação";
  if (iqa >= 90) return "Ótima";
  if (iqa >= 75) return "Boa";
  if (iqa >= 50) return "Atenção";
  return "Crítica";
}

export function getRiverStatusClassName(status: RiverStatus): string {
  switch (status) {
    case "Ótima": return "bg-primary text-white";
    case "Boa": return "bg-secondary text-text-primary";
    case "Atenção": return "bg-warning text-text-primary";
    case "Crítica": return "bg-contrast text-text-primary";
    default: return "bg-placeholder text-text-secondary";
  }
}

export function buildRivers(bodies: unknown[], collections: unknown[], qualities: unknown[]) {
  const rivers = new Map<string, River>();
  let ignoredBodies = 0;
  let ignoredCollections = 0;
  let ignoredQualities = 0;
  let invalidMetrics = 0;

  for (const body of bodies) {
    const riverId = id(field(body, "id"));
    if (!riverId || rivers.has(riverId)) {
      ignoredBodies++;
      continue;
    }
    const users = field(body, "users");
    rivers.set(riverId, {
      id: riverId,
      name: text(field(body, "nome")) ?? `Corpo hídrico ${riverId}`,
      location: text(field(body, "localizacao")) ?? "Localização não informada",
      userIds: Array.isArray(users) ? users.flatMap((user) => {
        const userId = id(field(user, "id"));
        return userId ? [userId] : [];
      }) : [],
      status: "Sem classificação",
      measurements: [],
      iqa: null,
      iqaValues: [],
      qualityRecordCount: 0,
    });
  }

  if (bodies.length > 0 && rivers.size === 0) {
    throw new Error("Os corpos hídricos recebidos estão em um formato inválido.");
  }

  const collectionIds = new Set<string>();
  for (const collection of collections) {
    const collectionId = id(field(collection, "id"));
    const riverId = bodyId(collection);
    const timestamp = readTimestamp(field(collection, "data"));
    if (!riverId || !collectionId || timestamp === null || collectionIds.has(collectionId)) {
      ignoredCollections++;
      continue;
    }
    const river = rivers.get(riverId);
    if (!river) continue;
    collectionIds.add(collectionId);
    const measurement: RiverMeasurement = {
      id: collectionId,
      timestamp,
      ph: metric(field(collection, "ph"), 14),
      turbidity: metric(field(collection, "turbidez")),
      dissolvedOxygen: metric(field(collection, "oxigenioDissolvido")),
    };
    if ([measurement.ph, measurement.turbidity, measurement.dissolvedOxygen].includes(null)) invalidMetrics++;
    river.measurements.push(measurement);
  }

  const qualityIds = new Set<string>();
  for (const quality of qualities) {
    const qualityId = id(field(quality, "id"));
    const riverId = bodyId(quality);
    if (!riverId || !qualityId || qualityIds.has(qualityId)) {
      ignoredQualities++;
      continue;
    }
    const river = rivers.get(riverId);
    if (!river) continue;
    qualityIds.add(qualityId);
    river.qualityRecordCount++;
    const iqa = metric(field(quality, "iqa") ?? record(quality).IQA, 100);
    if (iqa === null) ignoredQualities++;
    else river.iqaValues.push(iqa);
  }

  for (const river of rivers.values()) {
    river.measurements.sort((a, b) => a.timestamp - b.timestamp || Number(a.id) - Number(b.id));
    // No date exists on Qualidade: neither response order nor ID proves recency.
    river.iqa = river.qualityRecordCount === 1 ? river.iqaValues[0] ?? null : null;
    river.status = getRiverStatus(river.iqa);
  }

  const warnings: string[] = [];
  if (ignoredBodies) warnings.push(`${ignoredBodies} registro(s) de corpo hídrico inválido(s) ou duplicado(s) foram desconsiderados.`);
  if (ignoredCollections) warnings.push(`${ignoredCollections} coleta(s) sem identificação, vínculo ou data válida, ou duplicada(s), não puderam ser exibidas.`);
  if (ignoredQualities) warnings.push(`${ignoredQualities} registro(s) de IQA inválido(s), duplicado(s) ou sem vínculo não puderam ser exibidos.`);
  if (invalidMetrics) warnings.push("Algumas coletas têm parâmetros ausentes ou inválidos; esses valores aparecem como não informados.");
  return { rivers: Array.from(rivers.values()), warnings };
}

export function formatMeasurement(value: number | null | undefined): string {
  return value == null ? "Não informado" : value.toLocaleString("pt-BR", { maximumFractionDigits: 2 });
}

export function formatCollectionDate(timestamp: number): string {
  return new Date(timestamp).toLocaleString("pt-BR", { timeZone: "UTC", dateStyle: "short", timeStyle: "short" });
}
