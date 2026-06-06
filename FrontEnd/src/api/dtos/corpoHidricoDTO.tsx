import { UserDTO } from "./userDTO";

//requests
export interface CorpoHidricoCadastroDTO {
    nome: string;
    descricao: string;
    localizacao: string;
    tamanho: string;
    ehPrivado: boolean;
}


//Responses
export interface CorpoHidricoDTO {
    id: number;
    nome: string;
    localizacao: string;
    descricao: string;
    tamanho: string;
    ehPrivado: boolean;
    users : UserDTO[];
} 