// Response contract from ColetaEntity and MedicoesEntity in the backend.
export interface MedicaoDTO {
  id: number;
  coletaId: number;
  codigoMedicao: number | string;
  valor?: number | null;
  unidade: string;
  censurado: boolean;
  limite?: number | null;
}

export interface MonitoringColetaDTO {
  id: number;
  corpoHidricoId: number;
  dataHora: string;
  medicoes: MedicaoDTO[];
  latitude?: number | null;
  longitude?: number | null;
  profundidadeMetros?: number | null;
}
