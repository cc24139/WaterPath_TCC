export type RiverStatus = "Crítica" | "Atenção" | "Boa" | "Ótima" | "Sem classificação";

export type RiverMeasurement = {
  id: string;
  timestamp: number;
  ph: number | null;
  turbidity: number | null;
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
  iqaValues: number[];
  qualityRecordCount: number;
};
