export interface GerarCodigoDTO {
  usuarioEmail: string;
}

export interface ValidarCodigoDTO {
  usuarioEmail: string;
  codigo: string;
}

export interface GerarCodigoResponseDTO {
  codigo: string;
}

export interface ValidarCodigoResponseDTO {
  mensagem?: string;
}

export interface CodigoPendenteResponseDTO {
  pendente: boolean;
}
