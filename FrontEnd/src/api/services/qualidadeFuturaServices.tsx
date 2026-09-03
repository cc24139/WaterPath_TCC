import { routes } from "../routes";
import { QualidadeFuturaCadastroDTO } from "../dtos/qualidadeFuturaDTO";
import { apiFetch } from "../apiFetch";

export const qualidadeFuturaServices = {
  async create(data: QualidadeFuturaCadastroDTO): Promise<Response> {
    return apiFetch(routes.qualidadeFutura, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data),
    });
  },

  async getById(id: number): Promise<Response> {
    return apiFetch(`${routes.qualidadeFutura}${id}`);
  },

  async getAll(): Promise<Response> {
    return apiFetch(routes.qualidadeFutura);
  },

  async update(
    id: number,
    data: QualidadeFuturaCadastroDTO,
  ): Promise<Response> {
    return apiFetch(`${routes.qualidadeFutura}${id}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data),
    });
  },

  async remove(id: number): Promise<Response> {
    return apiFetch(`${routes.qualidadeFutura}${id}`, {
      method: "DELETE",
    });
  },
};
