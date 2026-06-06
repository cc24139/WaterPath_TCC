export interface ColetaRelationDTO {
  id: number;
}

export interface ColetaCadastroDTO {
  nome: string;
  data: string;
  ph: number;
  oxigenioDissolvido: number;
  turbidez: number;
  cloroResidual: number;
  floretos: number;
  coliformesTotais: number;
  escherichiaColi: boolean;
  corpoHidrico: ColetaRelationDTO;
}

export interface ColetaDTO {
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
  corpoHidrico?: ColetaRelationDTO;
}
