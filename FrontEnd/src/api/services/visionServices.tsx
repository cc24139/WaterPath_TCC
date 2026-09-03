import { routes } from "../routes";
import { VisionPredicaoDTO } from "../dtos/visionDTO";
import { apiFetch } from "../apiFetch";

export const visionServices = {
  async predizerImagem(data: VisionPredicaoDTO): Promise<Response> {
    return apiFetch(routes.vision, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data.pathImagem),
    });
  },
};
