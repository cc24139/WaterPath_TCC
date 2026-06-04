import { LoginDTO, LoginResponseDTO, UserRegisterDTO } from "../dtos/userDTO";
import {userServices} from "../services/userServices";
import {useState,useEffect} from "react";

interface state{
    user: LoginResponseDTO | null;
    loading: boolean;
    error: string | null;
}

export const useLogin = () => {
    const [loading,setLoading] = useState(false);
    const [error,setError] = useState<string | null>(null);

    const login = async (dados : LoginDTO) => {
        setLoading(true);
        setError(null);
        const response = await userServices.login(dados);
        console.log("Resposta do login:", response.status);
        console.log(response);
        if(response.status == 200){
            setLoading(false);
            return response.json();
        }
        else if(response.status == 401){
            setError("Email não confirmado. Por favor, verifique seu email para confirmar sua conta.");
            setLoading(false);
            return response;
        }
        else{
            setError("Falha no login. Verifique suas credenciais e tente novamente.");
            setLoading(false);
            return response;
        }
    };

    return { login, loading, error };
};

export const useRegister = () => {
    const [loading,setLoading] = useState(false);
    const [error,setError] = useState<string | null>(null);

    const register = async (dados : UserRegisterDTO) => {
        setLoading(true);
        setError(null);
        const response = await userServices.register(dados);
        console.log("Resposta do registro:", response.status);
        console.log(response);
        setLoading(false);
        if(response.status == 401){
            setError("Email já cadastrado. Por favor, utilize outro email.");
        }
        else if(response.status != 200){
            setError("Falha no registro. Verifique os dados e tente novamente.");
        }
    };
    return { register, loading, error };
}