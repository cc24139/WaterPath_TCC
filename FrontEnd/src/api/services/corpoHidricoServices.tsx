import { routes } from "../routes";
import { CorpoHidricoCadastroDTO } from "../dtos/corpoHidricoDTO";
import { getAuthHeaders } from "./authHeaders";

export const corpoHidricoServices = {
  async create(data: CorpoHidricoCadastroDTO): Promise<Response> {
    return fetch(routes.corpoHidrico, {
      method: "POST",
      headers: getAuthHeaders(true),
      body: JSON.stringify(data),
    });
  },

  async getById(id: number): Promise<Response> {
    return fetch(`${routes.corpoHidrico}${id}`, {
      headers: getAuthHeaders(),
    });
  },

  async getAll(): Promise<Response> {
    return fetch(routes.corpoHidrico, {
      headers: getAuthHeaders(),
    });
  },

  async getByUsuario(): Promise<Response> {
    return fetch(`${routes.corpoHidrico}usuario`, {
      headers: getAuthHeaders(),
    });
  },

  async getByUsuarioNome(nome: string): Promise<Response> {
    return fetch(`${routes.corpoHidrico}usuario/${nome}`, {
      headers: getAuthHeaders(),
    });
  },

  async update(id: number, data: CorpoHidricoCadastroDTO): Promise<Response> {
    return fetch(`${routes.corpoHidrico}${id}`, {
      method: "PUT",
      headers: getAuthHeaders(true),
      body: JSON.stringify(data),
    });
  },

  async remove(id: number): Promise<Response> {
    return fetch(`${routes.corpoHidrico}${id}`, {
      method: "DELETE",
      headers: getAuthHeaders(),
    });
  },
};
