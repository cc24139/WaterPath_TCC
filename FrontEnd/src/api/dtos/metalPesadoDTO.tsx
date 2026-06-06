export interface MetalPesadoRelationDTO {
  id: number;
}

export interface MetalPesadoCadastroDTO {
  nome: string;
  concentracao: number;
  unidade: string;
  coleta: MetalPesadoRelationDTO;
}

export interface MetalPesadoDTO {
  id: number;
  nome: string;
  concentracao: number;
  unidade: string;
  coleta?: MetalPesadoRelationDTO;
}
