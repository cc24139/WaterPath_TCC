import { LoginDTO, LoginResponseDTO } from "../dtos/userDTO";
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