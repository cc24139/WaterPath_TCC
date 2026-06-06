import { routes } from "../routes";
import { QualidadeFuturaCadastroDTO } from "../dtos/qualidadeFuturaDTO";
import { getAuthHeaders } from "./authHeaders";

export const qualidadeFuturaServices = {
  async create(data: QualidadeFuturaCadastroDTO): Promise<Response> {
    return fetch(routes.qualidadeFutura, {
      method: "POST",
      headers: getAuthHeaders(true),
      body: JSON.stringify(data),
    });
  },

  async getById(id: number): Promise<Response> {
    return fetch(`${routes.qualidadeFutura}${id}`, {
      headers: getAuthHeaders(),
    });
  },

  async getAll(): Promise<Response> {
    return fetch(routes.qualidadeFutura, {
      headers: getAuthHeaders(),
    });
  },

  async update(
    id: number,
    data: QualidadeFuturaCadastroDTO,
  ): Promise<Response> {
    return fetch(`${routes.qualidadeFutura}${id}`, {
      method: "PUT",
      headers: getAuthHeaders(true),
      body: JSON.stringify(data),
    });
  },

  async remove(id: number): Promise<Response> {
    return fetch(`${routes.qualidadeFutura}${id}`, {
      method: "DELETE",
      headers: getAuthHeaders(),
    });
  },
};
