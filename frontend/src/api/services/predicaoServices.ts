import { apiFetch } from "../apiFetch";
import { routes } from "../routes";
import type { AmostraDTO } from "../dtos/predicaoDTO";

export const predicaoServices = {
  async create(coletaId: number, image: File, sample: AmostraDTO): Promise<Response> {
    const body = new FormData();
    body.append("coletaId", String(coletaId));
    body.append("image", image);
    body.append("data", JSON.stringify(sample));
    return apiFetch(routes.predicoes, { method: "POST", body }, { redirectOnUnauthorized: false });
  },
};
