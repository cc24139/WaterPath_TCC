import { routes } from "../routes";
import { VisionPredicaoDTO } from "../dtos/visionDTO";
import { apiFetch } from "../apiFetch";

export const visionServices = {
  async predizerImagem(data: VisionPredicaoDTO): Promise<Response> {
    const body = new FormData();
    body.append("coletaId", String(data.coletaId));
    body.append("file", data.image);
    return apiFetch(routes.vision, {
      method: "POST",
      body,
    });
  },
};
