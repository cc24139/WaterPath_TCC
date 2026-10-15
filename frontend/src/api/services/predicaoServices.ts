import { apiFetch } from "../apiFetch";
import { routes } from "../routes";
import type { AmostraDTO } from "../dtos/predicaoDTO";

export const predicaoServices = {
  async create(coletaId: number, image: File, sample: AmostraDTO): Promise<Response> {
    const body = new FormData();
    body.append("coletaId", String(coletaId));
    body.append("image", image);
    // Contrato de domínio da API principal; o cliente não conhece o multipart do serviço interno de IA.
    body.append("amostra", JSON.stringify(sample));
    return apiFetch(routes.predicoes, { method: "POST", body }, { redirectOnUnauthorized: false });
  },
};
