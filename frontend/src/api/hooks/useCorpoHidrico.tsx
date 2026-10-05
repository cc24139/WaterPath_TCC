import { useCallback, useRef, useState } from "react";
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

export type WaterBodyQueryStatus = "idle" | "loading" | "success" | "not-found" | "unauthenticated" | "forbidden" | "error";

export const useGetById = () => {
  const [loading, setLoading] = useState(false);
  const [status, setStatus] = useState<WaterBodyQueryStatus>("idle");
  const [error, setError] = useState<string | null>(null);
  const [corpoHidrico, setCorpoHidrico] = useState<CorpoHidricoDTO | null>(null);
  const requestId = useRef(0);
  const getCorpoHidricoById = useCallback(async (id: number, signal?: AbortSignal) => {
    const currentRequest = ++requestId.current;
    const isCurrent = () => currentRequest === requestId.current && !signal?.aborted;
    if (signal?.aborted) return;
    setCorpoHidrico(null);
    setError(null);
    if (!Number.isInteger(id) || id <= 0 || id > 2147483647) {
      setStatus("not-found"); setLoading(false); return;
    }
    setLoading(true);
    setStatus("loading");
    try {
      const response = await corpoHidricoServices.getById(id, signal);
      if (!isCurrent()) return;
      if (response.status === 401) { setStatus("unauthenticated"); return; }
      if (response.status === 403) { setStatus("forbidden"); return; }
      if (response.status === 404) { setStatus("not-found"); return; }
      if (!response.ok) throw new Error("Falha na consulta");
      const data: CorpoHidricoDTO = await response.json();
      if (!data || data.id !== id || typeof data.nome !== "string" || typeof data.localizacao !== "string") throw new Error("Resposta inv?lida");
      if (!isCurrent()) return;
      setCorpoHidrico(data);
      setStatus("success");
    } catch {
      if (!isCurrent()) return;
      setError("N?o foi poss?vel carregar os dados. Tente novamente.");
      setStatus("error");
    } finally {
      if (isCurrent()) setLoading(false);
    }
  }, []);
  return { loading, status, error, corpoHidrico, getCorpoHidricoById };
};

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
