export interface QualidadeFuturaRelationDTO {
  id: number;
}

export interface QualidadeFuturaCadastroDTO {
  nome: string;
  data: string;
  ph: number;
  oxigenioDissolvido: number;
  turbidez: number;
  cloroResidual: number;
  floretos: number;
  coliformesTotais: number;
  escherichiaColi: boolean;
  corpoHidrico: QualidadeFuturaRelationDTO;
}

export interface QualidadeFuturaDTO {
  id: number;
  nome: string;
  data: string;
  ph: number;
  oxigenioDissolvido: number;
  turbidez: number;
  cloroResidual: number;
  floretos: number;
  coliformesTotais: number;
  escherichiaColi: boolean;
  corpoHidrico?: QualidadeFuturaRelationDTO;
}
