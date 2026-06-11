
export interface ImagemRelationDTO {
  id: number;
}

export interface ImagemCadastroDTO {
  idCorpoHidrico?: number;
  idColeta?: number;
  dateTime: Date;
  Imagem : File
  
}

export interface ImagemDTO {
  id: number;
  url: string;
  dataUpload?: string;
  corpoHidrico?: ImagemRelationDTO;
  coleta?: ImagemRelationDTO;
}
