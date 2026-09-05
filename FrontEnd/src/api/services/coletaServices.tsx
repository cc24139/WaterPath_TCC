import { routes } from "../routes";
import { ColetaCadastroDTO } from "../dtos/coletaDTO";
import { apiFetch } from "../apiFetch";

export const coletaServices = {
  async create(data: ColetaCadastroDTO): Promise<Response> {
    return apiFetch(routes.coleta, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data),
    });
  },

  async getById(id: number): Promise<Response> {
    return apiFetch(`${routes.coleta}${id}`);
  },

  async getAll(signal?: AbortSignal): Promise<Response> {
    return apiFetch(routes.coleta, { signal }, { authenticated: false });
  },

  async update(id: number, data: ColetaCadastroDTO): Promise<Response> {
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
