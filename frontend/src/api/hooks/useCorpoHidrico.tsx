import { useState } from "react";
import { corpoHidricoServices } from "../services/corpoHidricoServices";
import type {
  CorpoHidricoCadastroDTO,
  CorpoHidricoDTO,
} from "../dtos/corpoHidricoDTO";

type CadastroResult =
  | { ok: true }
  | { ok: false; message: string };

export const useCadastro = () => {
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const cadastrarCorpoHidrico = async (
    dados: CorpoHidricoCadastroDTO
  ): Promise<CadastroResult> => {
    setIsLoading(true);
    setError(null);

    try {
      const response = await corpoHidricoServices.create(dados);

      if (!response.ok) {
        const responseMessage = await response.text();

        const message =
          responseMessage || "Erro ao cadastrar corpo hídrico.";

        setError(message);

        return {
          ok: false,
          message,
        };
      }

      return { ok: true };
    } catch {
      const message = "Não foi possível conectar ao servidor.";

      setError(message);

      return {
        ok: false,
        message,
      };
    } finally {
      setIsLoading(false);
    }
  };

  return {
    cadastrarCorpoHidrico,
    isLoading,
    error,
  };
};

export const useGetById = () => {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [corpoHidrico, setCorpoHidrico] = useState<CorpoHidricoDTO | null>(null);

    const getCorpoHidricoById = async (id: number) => {
        if(id < 0) {
            setError("ID inválido");
            setLoading(false);
            return;
        }
        setLoading(true);
        setError(null);
        const response = await corpoHidricoServices.getById(id);
        if (!response.ok) {
            setError("Erro ao buscar corpo hidrico");
        } else {
            const data: CorpoHidricoDTO = await response.json();
            setCorpoHidrico(data);
        }
        setLoading(false);
    };

    return { loading, error, corpoHidrico, getCorpoHidricoById };
}

export const useGetAll = () => {
    const [loading,setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [corposHidricos, setCorposHidricos] = useState<CorpoHidricoDTO[]>([]);

    const getCorposHidricos = async () => {
        setLoading(true);
        setError(null);
        const response = await corpoHidricoServices.getAll();
        if (!response.ok) {
            setError("Erro ao buscar corpos hidricos");
        } else {
            const data: CorpoHidricoDTO[] = await response.json();
            setCorposHidricos(data);
        }
        setLoading(false);
    };

    return { loading, error, corposHidricos, getCorposHidricos };
}

export const useGetByUsuario = () => {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [corposHidricos, setCorposHidricos] = useState<CorpoHidricoDTO[]>([]);

    const getCorposHidricosByUsuario = async () => {
        setLoading(true);
        setError(null);
        const response = await corpoHidricoServices.getByUsuario();
        if (!response.ok) {
            setError("Erro ao buscar corpos hidricos");
        } else {
            const data: CorpoHidricoDTO[] = await response.json();
            setCorposHidricos(data);
        }
        setLoading(false);
    };

    return { loading, error, corposHidricos, getCorposHidricosByUsuario };
}

export const useGetByUsuarioNome = () => {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [corposHidricos, setCorposHidricos] = useState<CorpoHidricoDTO[]>([]);

    const getCorposHidricosByUsuarioNome = async (nome: string) => {
        setLoading(true);
        setError(null);
        const response = await corpoHidricoServices.getByUsuarioNome(nome);
        if (!response.ok) {
            setError("Erro ao buscar corpos hidricos");
        } else {
            const data: CorpoHidricoDTO[] = await response.json();
            setCorposHidricos(data);
        }
        setLoading(false);
    };

    return { loading, error, corposHidricos, getCorposHidricosByUsuarioNome };
}
