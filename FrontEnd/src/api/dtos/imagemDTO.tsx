export interface ImagemRelationDTO {
  id: number;
}

export interface ImagemCadastroDTO {
  url: string;
  corpoHidrico?: ImagemRelationDTO;
  coleta?: ImagemRelationDTO;
}

export interface ImagemDTO {
  id: number;
  url: string;
  dataUpload?: string;
  corpoHidrico?: ImagemRelationDTO;
  coleta?: ImagemRelationDTO;
}
