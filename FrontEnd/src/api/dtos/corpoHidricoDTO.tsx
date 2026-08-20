import { UserDTO } from "./userDTO";

//requests
export interface CorpoHidricoCadastroDTO {
  nome: string;
  localizacao: string;
  tamanho: number;
  ehPrivado: boolean;
}


//Responses
export interface CorpoHidricoDTO {
  id: number;
  nome: string;
  localizacao: string;
  tamanho: number;
  users: UserDTO[];
}
