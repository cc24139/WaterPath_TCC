import { apiFetch } from "../apiFetch";
import { routes } from "../routes";
import type { AmostraDTO } from "../dtos/predicaoDTO";

export const predicaoServices = {
  async getById(id: number, signal?: AbortSignal): Promise<Response> {
    return apiFetch(`${routes.predicoes}${id}`, { signal }, { redirectOnUnauthorized: false });
  },
  async getResultImage(id: number, signal?: AbortSignal): Promise<Response> {
    return apiFetch(`${routes.predicoes}${id}/imagem`, { signal }, { redirectOnUnauthorized: false });
  },
  async getCollectionToResume(coletaId: number, signal?: AbortSignal): Promise<Response> {
    return apiFetch(`${routes.predicoes}coletas/${coletaId}/retomada`, { signal }, { redirectOnUnauthorized: false });
  },
  async create(coletaId: number, image: File, sample: AmostraDTO, requestKey?: string): Promise<Response> {
    const body = new FormData();
    body.append("coletaId", String(coletaId));
    body.append("image", image);
    // Contrato de domínio da API principal; o cliente não conhece o multipart do serviço interno de IA.
    body.append("amostra", JSON.stringify(sample));
    return apiFetch(routes.predicoes, { method: "POST", body,
      headers: requestKey ? { "Idempotency-Key": requestKey } : undefined }, { redirectOnUnauthorized: false });
  },
};
