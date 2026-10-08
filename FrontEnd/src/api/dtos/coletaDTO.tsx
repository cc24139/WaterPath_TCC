import type { MonitoringColetaDTO } from "./monitoringDTO";

export interface MedicaoCadastroDTO {
  codigoMedicao: string;
  valor: number;
  unidade: string;
}

export interface MetalMedidoCadastroDTO {
  nome: string;
  concentracao: number;
  unidade: string;
}

export interface ColetaCadastroDTO {
  corpoHidricoId: number;
  dataHora: string;
  medicoes: MedicaoCadastroDTO[];
  metaisPesados?: MetalMedidoCadastroDTO[];
}

export type ColetaAtualizacaoDTO = Omit<ColetaCadastroDTO, "metaisPesados">;

export interface ColetaDTO extends MonitoringColetaDTO {
  metaisPesados?: MetalMedidoCadastroDTO[];
  responsavelId?: number | null;
  responsavelNome?: string | null;
}
