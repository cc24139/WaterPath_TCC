export interface CianoBacteriaRelationDTO {
  id: number;
}

export interface CianoBacteriaCadastroDTO {
  tipo: string;
  concentracao: number;
  unidade: string;
  coleta: CianoBacteriaRelationDTO;
}

export interface CianoBacteriaDTO {
  id: number;
  tipo: string;
  concentracao: number;
  unidade: string;
  coleta?: CianoBacteriaRelationDTO;
}
