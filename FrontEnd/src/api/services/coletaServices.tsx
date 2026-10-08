import { routes } from "../routes";
import type { ColetaAtualizacaoDTO, ColetaCadastroDTO } from "../dtos/coletaDTO";
import { apiFetch } from "../apiFetch";

export const coletaServices = {
  async getByWaterBody(id: number, signal?: AbortSignal): Promise<Response> {
    return apiFetch(`${routes.coleta}corpo-hidrico/${id}`, { signal }, { redirectOnUnauthorized: false });
  },

  async getByPeriod(id: number, from: string, to: string, signal?: AbortSignal): Promise<Response> {
    const query = new URLSearchParams({ dataInicio: from, dataFim: to });
    return apiFetch(`${routes.coleta}periodo/${id}?${query}`, { signal }, { redirectOnUnauthorized: false });
  },

  async create(data: ColetaCadastroDTO): Promise<Response> {
    return apiFetch(routes.coleta, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data),
    }, { redirectOnUnauthorized: false });
  },

  async getById(id: number): Promise<Response> {
    return apiFetch(`${routes.coleta}${id}`);
  },

  async getAll(signal?: AbortSignal): Promise<Response> {
    return apiFetch(routes.coleta, { signal }, { authenticated: false });
  },

  async update(id: number, data: ColetaAtualizacaoDTO): Promise<Response> {
    return apiFetch(`${routes.coleta}${id}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data),
    });
  },

  async remove(id: number): Promise<Response> {
    return apiFetch(`${routes.coleta}${id}`, {
      method: "DELETE",
    });
  },
};
