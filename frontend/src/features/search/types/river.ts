export type RiverStatus = "Crítica" | "Atenção" | "Boa" | "Ótima" | "Sem classificação";

export type RiverMeasurement = {
  id: string;
  timestamp: number;
  ph: number | null;
  conductivity: number | null;
  dissolvedOxygen: number | null;
};

export type River = {
  id: string;
  name: string;
  location: string;
  status: RiverStatus;
  userIds: string[];
  measurements: RiverMeasurement[];
  iqa: number | null;
  iqaCollectedAt?: number;
  iqaSamples: { collectionId: string; timestamp: number; value: number }[];
  iqaValues: number[];
  qualityRecordCount: number;
};
