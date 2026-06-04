import { LoginResponseDTO } from "../dtos/userDTO";
import {userServices} from "../services/userServices";
import {useState,useEffect} from "react";

interface state{
    user: LoginResponseDTO | null;
    loading: boolean;
    error: string | null;
}

export const useLogin = (email: string, password: string) => {
    const [state, setState] = useState<state>({
        user: null,
        loading: false,
        error: null
    });
    const login = async () => {
        try{
            const data = await userServices.login({email, password});
            setState({user: data, loading: false, error: null});
        }catch (error ){
            setState({user: null, loading: false, error:(error as Error).message});
        }
    };
    return {state, login};
}