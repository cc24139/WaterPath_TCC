export interface QualidadeRelationDTO {
  id: number;
}

export interface QualidadeCadastroDTO {
  corpoHidrico: QualidadeRelationDTO;
  iqa: number;
  qualidadeFutura: QualidadeRelationDTO;
}

export interface QualidadeDTO {
  id: number;
  iqa: number;
  corpoHidrico?: QualidadeRelationDTO;
  qualidadeFutura?: QualidadeRelationDTO;
}
