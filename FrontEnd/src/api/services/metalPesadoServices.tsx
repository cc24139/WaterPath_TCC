import { routes } from "../routes";
import { MetalPesadoCadastroDTO } from "../dtos/metalPesadoDTO";
import { getAuthHeaders } from "./authHeaders";

export const metalPesadoServices = {
  async create(data: MetalPesadoCadastroDTO): Promise<Response> {
    return fetch(routes.metalPesado, {
      method: "POST",
      headers: getAuthHeaders(true),
      body: JSON.stringify(data),
    });
  },

  async getById(id: number): Promise<Response> {
    return fetch(`${routes.metalPesado}${id}`, {
      headers: getAuthHeaders(),
    });
  },

  async getAll(): Promise<Response> {
    return fetch(routes.metalPesado, {
      headers: getAuthHeaders(),
    });
  },

  async update(id: number, data: MetalPesadoCadastroDTO): Promise<Response> {
    return fetch(`${routes.metalPesado}${id}`, {
      method: "PUT",
      headers: getAuthHeaders(true),
      body: JSON.stringify(data),
    });
  },

  async remove(id: number): Promise<Response> {
    return fetch(`${routes.metalPesado}${id}`, {
      method: "DELETE",
      headers: getAuthHeaders(),
    });
  },
};
