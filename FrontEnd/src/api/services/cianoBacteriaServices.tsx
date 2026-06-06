import { routes } from "../routes";
import { CianoBacteriaCadastroDTO } from "../dtos/cianoBacteriaDTO";
import { getAuthHeaders } from "./authHeaders";

export const cianoBacteriaServices = {
  async create(data: CianoBacteriaCadastroDTO): Promise<Response> {
    return fetch(routes.cianoBacteria, {
      method: "POST",
      headers: getAuthHeaders(true),
      body: JSON.stringify(data),
    });
  },

  async getById(id: number): Promise<Response> {
    return fetch(`${routes.cianoBacteria}${id}`, {
      headers: getAuthHeaders(),
    });
  },

  async getAll(): Promise<Response> {
    return fetch(routes.cianoBacteria, {
      headers: getAuthHeaders(),
    });
  },

  async update(id: number, data: CianoBacteriaCadastroDTO): Promise<Response> {
    return fetch(`${routes.cianoBacteria}${id}`, {
      method: "PUT",
      headers: getAuthHeaders(true),
      body: JSON.stringify(data),
    });
  },

  async remove(id: number): Promise<Response> {
    return fetch(`${routes.cianoBacteria}${id}`, {
      method: "DELETE",
      headers: getAuthHeaders(),
    });
  },
};
