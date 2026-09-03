import { routes } from "../routes";
import { GerarCodigoDTO, ValidarCodigoDTO } from "../dtos/codigoDTO";
import { apiFetch } from "../apiFetch";

export const codigoServices = {
  async gerar(data: GerarCodigoDTO): Promise<Response> {
    return apiFetch(
      `${routes.codigo}gerar`,
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(data),
      },
      { authenticated: false }
    );
  },

  async validar(data: ValidarCodigoDTO): Promise<Response> {
    return apiFetch(
      `${routes.codigo}validar`,
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(data),
      },
      { authenticated: false }
    );
  },

  async verificarPendente(usuarioEmail: string): Promise<Response> {
    return apiFetch(
      `${routes.codigo}pendente/${encodeURIComponent(usuarioEmail)}`,
      {},
      { authenticated: false }
    );
  },
};
