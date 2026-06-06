import { routes } from "../routes";
import { ImagemCadastroDTO } from "../dtos/imagemDTO";
import { getAuthHeaders } from "./authHeaders";

export const imagemServices = {
  async create(data: ImagemCadastroDTO): Promise<Response> {
    return fetch(routes.imagem, {
      method: "POST",
      headers: getAuthHeaders(true),
      body: JSON.stringify(data),
    });
  },

  async getById(id: number): Promise<Response> {
    return fetch(`${routes.imagem}${id}`, {
      headers: getAuthHeaders(),
    });
  },

  async getAll(): Promise<Response> {
    return fetch(routes.imagem, {
      headers: getAuthHeaders(),
    });
  },

  async update(id: number, data: ImagemCadastroDTO): Promise<Response> {
    return fetch(`${routes.imagem}${id}`, {
      method: "PUT",
      headers: getAuthHeaders(true),
      body: JSON.stringify(data),
    });
  },

  async remove(id: number): Promise<Response> {
    return fetch(`${routes.imagem}${id}`, {
      method: "DELETE",
      headers: getAuthHeaders(),
    });
  },
};
