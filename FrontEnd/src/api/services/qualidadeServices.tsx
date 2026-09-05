import { routes } from "../routes";
import { QualidadeCadastroDTO } from "../dtos/qualidadeDTO";
import { apiFetch } from "../apiFetch";

export const qualidadeServices = {
  async create(data: QualidadeCadastroDTO): Promise<Response> {
    return apiFetch(routes.qualidade, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data),
    });
  },

  async getById(id: number): Promise<Response> {
    return apiFetch(`${routes.qualidade}${id}`);
  },

  async getAll(signal?: AbortSignal): Promise<Response> {
    return apiFetch(routes.qualidade, { signal }, { authenticated: false });
  },

  async update(id: number, data: QualidadeCadastroDTO): Promise<Response> {
    return apiFetch(`${routes.qualidade}${id}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data),
    });
  },

  async remove(id: number): Promise<Response> {
    return apiFetch(`${routes.qualidade}${id}`, {
      method: "DELETE",
    });
  },
};
