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
        console.log("Login iniciado com dados:", dados);
        setLoading(true);
        setError(null);
        try{
            const response = await userServices.login(dados);
            return response;
        } catch (err) {
            setError((err as Error).message);
        } finally {
            setLoading(false);
        }
    };

    return { login, loading, error };
};