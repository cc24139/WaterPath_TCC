export type RiverStatus = "Crítica" | "Atenção" | "Boa" | "Ótima";

export type River = {
  id: string;
  name: string;
  location: string;
  status: RiverStatus;
  ph: number;
  turbidity: number;
  dissolvedOxygen: number;
  temperature: number;
  iqa: number;
  chartData: number[];
  imageUrl?: string;
};
