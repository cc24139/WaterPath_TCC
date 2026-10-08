import type { ColetaCadastroDTO } from "../../../api/dtos/coletaDTO";
import type { AmostraDTO, PredicaoDTO } from "../../../api/dtos/predicaoDTO";

type Submission = {
  collection: ColetaCadastroDTO;
  sample: AmostraDTO;
  savedCollectionId: number | null;
  image: File | null;
};
type SubmissionOperations = {
  createCollection: (data: ColetaCadastroDTO) => Promise<number>;
  analyze: (id: number, image: File, sample: AmostraDTO) => Promise<PredicaoDTO>;
  onCollectionSaved: (id: number) => void;
};

export async function submitAnalysis(input: Submission, operations: SubmissionOperations) {
  const id = input.savedCollectionId ?? await operations.createCollection(input.collection);
  // Preserve the confirmed ID before the request to IA, including when that request fails.
  operations.onCollectionSaved(id);
  return input.image ? operations.analyze(id, input.image, input.sample) : null;
}

export async function readSubmissionResponse(response: Response): Promise<unknown> {
  const text = await response.text();
  let data: unknown;
  try { data = JSON.parse(text); } catch { data = null; }
  if (!response.ok) {
    if (response.status === 401) throw new Error("Sua sessão expirou. Entre novamente para continuar.");
    const detail = data && typeof data === "object" && "detail" in data ? data.detail : null;
    const message = typeof detail === "string" ? detail : typeof data === "string" ? data : !text.includes("<") ? text : "";
    throw new Error(message.slice(0, 300) || "Não foi possível concluir o envio. Tente novamente.");
  }
  return data;
}
