export type AnalysisStatus =
  | "Processando"
  | "Concluída"
  | "Atenção"
  | "Rascunho";

export type AnalysisStatusFilter = "Todos os status" | AnalysisStatus;

export type AnalysisSortOption =
  | "newest"
  | "oldest"
  | "water-body"
  | "status";

export interface UserAnalysis {
  id: string;
  waterBodyId: string;
  waterBodyName: string;
  city: string;
  state: string;
  collectionPoint: string;
  analyzedAt: string;
  status: AnalysisStatus;
}

export interface AnalysisStats {
  total: number;
  processing: number;
  completed: number;
  drafts: number;
}

export type AnalysisMenuAction = "details" | "edit" | "duplicate" | "delete";
