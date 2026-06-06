export interface LoginDTO {
  email: string;
  senha: string;
}

export interface LoginResponseDTO {
    id: number;
    token: string;
    email: string;
    nome: string;
} 


export interface UserRegisterDTO {
  nome: string;
  email: string;
  senha: string;
}

export interface UserDTO {
    id: number;
    nome: string;
    email: string;
}
