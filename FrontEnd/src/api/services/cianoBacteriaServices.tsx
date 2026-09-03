import { routes } from "../routes";
import { CianoBacteriaCadastroDTO } from "../dtos/cianoBacteriaDTO";
import { apiFetch } from "../apiFetch";

export const cianoBacteriaServices = {
  async create(data: CianoBacteriaCadastroDTO): Promise<Response> {
    return apiFetch(routes.cianoBacteria, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data),
    });
  },

  async getById(id: number): Promise<Response> {
    return apiFetch(`${routes.cianoBacteria}${id}`);
  },

  async getAll(): Promise<Response> {
    return apiFetch(routes.cianoBacteria);
  },

  async update(id: number, data: CianoBacteriaCadastroDTO): Promise<Response> {
    return apiFetch(`${routes.cianoBacteria}${id}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data),
    });
  },

  async remove(id: number): Promise<Response> {
    return apiFetch(`${routes.cianoBacteria}${id}`, {
      method: "DELETE",
    });
  },
};
