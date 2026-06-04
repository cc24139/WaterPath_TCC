export interface LoginDTO {
    email: string;
    password: string;
}

export interface LoginResponseDTO {
    id: number;
    token: string;
    email: string;
    nome: string;
}   
