import { routes } from "../routes";
import { GerarCodigoDTO, ValidarCodigoDTO } from "../dtos/codigoDTO";

export const codigoServices = {
  async gerar(data: GerarCodigoDTO): Promise<Response> {
    return fetch(`${routes.codigo}gerar`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(data),
    });
  },

  async validar(data: ValidarCodigoDTO): Promise<Response> {
    return fetch(`${routes.codigo}validar`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(data),
    });
  },

  async verificarPendente(usuarioEmail: string): Promise<Response> {
    return fetch(`${routes.codigo}pendente/${usuarioEmail}`);
  },
};
