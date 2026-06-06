import { routes } from "../routes";
import { QualidadeCadastroDTO } from "../dtos/qualidadeDTO";
import { getAuthHeaders } from "./authHeaders";

export const qualidadeServices = {
  async create(data: QualidadeCadastroDTO): Promise<Response> {
    return fetch(routes.qualidade, {
      method: "POST",
      headers: getAuthHeaders(true),
      body: JSON.stringify(data),
    });
  },

  async getById(id: number): Promise<Response> {
    return fetch(`${routes.qualidade}${id}`, {
      headers: getAuthHeaders(),
    });
  },

  async getAll(): Promise<Response> {
    return fetch(routes.qualidade, {
      headers: getAuthHeaders(),
    });
  },

  async update(id: number, data: QualidadeCadastroDTO): Promise<Response> {
    return fetch(`${routes.qualidade}${id}`, {
      method: "PUT",
      headers: getAuthHeaders(true),
      body: JSON.stringify(data),
    });
  },

  async remove(id: number): Promise<Response> {
    return fetch(`${routes.qualidade}${id}`, {
      method: "DELETE",
      headers: getAuthHeaders(),
    });
  },
};
