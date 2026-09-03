import { routes } from "../routes";
import { MetalPesadoCadastroDTO } from "../dtos/metalPesadoDTO";
import { apiFetch } from "../apiFetch";

export const metalPesadoServices = {
  async create(data: MetalPesadoCadastroDTO): Promise<Response> {
    return apiFetch(routes.metalPesado, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data),
    });
  },

  async getById(id: number): Promise<Response> {
    return apiFetch(`${routes.metalPesado}${id}`);
  },

  async getAll(): Promise<Response> {
    return apiFetch(routes.metalPesado);
  },

  async update(id: number, data: MetalPesadoCadastroDTO): Promise<Response> {
    return apiFetch(`${routes.metalPesado}${id}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data),
    });
  },

  async remove(id: number): Promise<Response> {
    return apiFetch(`${routes.metalPesado}${id}`, {
      method: "DELETE",
    });
  },
};
