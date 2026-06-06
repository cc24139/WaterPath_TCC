import { routes } from "../routes";
import { VisionPredicaoDTO } from "../dtos/visionDTO";
import { getAuthHeaders } from "./authHeaders";

export const visionServices = {
  async predizerImagem(data: VisionPredicaoDTO): Promise<Response> {
    return fetch(routes.vision, {
      method: "POST",
      headers: getAuthHeaders(true),
      body: JSON.stringify(data.pathImagem),
    });
  },
};
