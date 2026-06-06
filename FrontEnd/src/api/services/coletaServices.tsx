import { routes } from "../routes";
import { ColetaCadastroDTO } from "../dtos/coletaDTO";
import { getAuthHeaders } from "./authHeaders";

export const coletaServices = {
  async create(data: ColetaCadastroDTO): Promise<Response> {
    return fetch(routes.coleta, {
      method: "POST",
      headers: getAuthHeaders(true),
      body: JSON.stringify(data),
    });
  },

  async getById(id: number): Promise<Response> {
    return fetch(`${routes.coleta}${id}`, {
      headers: getAuthHeaders(),
    });
  },

  async getAll(): Promise<Response> {
    return fetch(routes.coleta, {
      headers: getAuthHeaders(),
    });
  },

  async update(id: number, data: ColetaCadastroDTO): Promise<Response> {
    return fetch(`${routes.coleta}${id}`, {
      method: "PUT",
      headers: getAuthHeaders(true),
      body: JSON.stringify(data),
    });
  },

  async remove(id: number): Promise<Response> {
    return fetch(`${routes.coleta}${id}`, {
      method: "DELETE",
      headers: getAuthHeaders(),
    });
  },
};
