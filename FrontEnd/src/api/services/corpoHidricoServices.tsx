import { routes } from "../routes";
import { CorpoHidricoCadastroDTO } from "../dtos/corpoHidricoDTO";
import { apiFetch } from "../apiFetch";

export const corpoHidricoServices = {
  async create(data: CorpoHidricoCadastroDTO): Promise<Response> {
    return apiFetch(routes.corpoHidrico, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data),
    });
  },

  async getById(id: number): Promise<Response> {
    return apiFetch(`${routes.corpoHidrico}${id}`);
  },

  async getAll(signal?: AbortSignal): Promise<Response> {
    return apiFetch(routes.corpoHidrico, { signal }, { authenticated: false });
  },

  async getByUsuario(): Promise<Response> {
    return apiFetch(`${routes.corpoHidrico}usuario`);
  },

  async getByUsuarioNome(nome: string): Promise<Response> {
    return apiFetch(
      `${routes.corpoHidrico}usuario/${encodeURIComponent(nome)}`
    );
  },

  async update(id: number, data: CorpoHidricoCadastroDTO): Promise<Response> {
    return apiFetch(`${routes.corpoHidrico}${id}`, {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data),
    });
  },

  async remove(id: number): Promise<Response> {
    return apiFetch(`${routes.corpoHidrico}${id}`, {
      method: "DELETE",
    });
  },
};
