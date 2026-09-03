import type {
    LoginDTO,
    LoginResponseDTO,
    UserRegisterDTO,
} from "../dtos/userDTO";
import { userServices } from "../services/userServices";
import { useState } from "react";

export type RegisterResult =
    | { ok: true; status: number; message?: string }
    | { ok: false; status?: number; message: string };

export const useLogin = () => {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const login = async (
        dados: LoginDTO
    ): Promise<LoginResponseDTO | Response> => {
        setLoading(true);
        setError(null);

        try {
            const response = await userServices.login(dados);

            if (response.ok) {
                return response.json();
            }

            if (response.status === 403) {
                setError(
                    "Email não confirmado. Por favor, verifique seu email para confirmar sua conta."
                );
            } else {
                setError(
                    "Falha no login. Verifique suas credenciais e tente novamente."
                );
            }

            return response;
        } finally {
            setLoading(false);
        }
    };

    return { login, loading, error };
};

export const useRegister = () => {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const register = async (
        dados: UserRegisterDTO
    ): Promise<RegisterResult> => {
        setLoading(true);
        setError(null);

        try {
            const response = await userServices.register(dados);
            const responseMessage = await getResponseMessage(response);

            if (!response.ok) {
                const message =
                    responseMessage ??
                    (response.status === 409
                        ? "Email já cadastrado ou com confirmação pendente."
                        : "Falha no cadastro. Verifique os dados e tente novamente.");

                setError(message);

                return {
                    ok: false,
                    status: response.status,
                    message,
                };
            }

            return {
                ok: true,
                status: response.status,
                message: responseMessage ?? undefined,
            };
        } catch {
            const message = "Não foi possível conectar ao servidor.";

            setError(message);

            return {
                ok: false,
                message,
            };
        } finally {
            setLoading(false);
        }
    };

    return { register, loading, error };
};

async function getResponseMessage(response: Response) {
    const responseBody = await response.text();

    if (!responseBody) {
        return null;
    }

    try {
        const data = JSON.parse(responseBody) as { mensagem?: unknown };

        return typeof data.mensagem === "string" ? data.mensagem : null;
    } catch {
        return responseBody;
    }
}
