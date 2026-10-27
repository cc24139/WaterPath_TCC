"use client";

import { useEffect, useRef, useState } from "react";
import { coletaServices } from "@/api/services/coletaServices";
import { predicaoServices } from "@/api/services/predicaoServices";
import type { PredicaoDTO } from "@/api/dtos/predicaoDTO";
import { getAuthSession } from "@/features/auth/utils/authSession";
import { initialAddAnalysisForm } from "../constants/addAnalysisOptions";
import type { AddAnalysisErrors, AddAnalysisFieldName, MetalSymbol } from "../types/addAnalysis";
import { buildAnalysisPayload, validateAnalysis, validateImage } from "../utils/analysisForm";
import { readSubmissionResponse, submitAnalysis } from "../utils/submitAnalysis";
import { readPrediction } from "../utils/predictionResponse";
import { restoreCollectionForm } from "../utils/restoreCollection";
import { clearRequestKey, imageFingerprint, requestKey } from "../utils/idempotencyKeys";

export function useAddAnalysisForm(resumeCollectionId: number | null = null, fixedWaterBodyId: number | null = null) {
  const initialForm = () => ({ ...initialAddAnalysisForm, waterBody: fixedWaterBodyId ? String(fixedWaterBodyId) : "" });
  const [form, setForm] = useState(initialForm);
  const [errors, setErrors] = useState<AddAnalysisErrors>({});
  const [image, setImage] = useState<File | null>(null);
  const [imagePreviewUrl, setImagePreviewUrl] = useState("");
  const objectUrlRef = useRef<string | null>(null);
  const [savedCollectionId, setSavedCollectionId] = useState<number | null>(null);
  const savedIdRef = useRef<number | null>(null);
  const [prediction, setPrediction] = useState<PredicaoDTO | null>(null);
  const [status, setStatus] = useState<"idle" | "restoring" | "saving" | "analyzing">(resumeCollectionId ? "restoring" : "idle");
  const [feedback, setFeedback] = useState<{ type: "error" | "success"; message: string } | null>(null);
  const busyRef = useRef(false);

  useEffect(() => () => { if (objectUrlRef.current) URL.revokeObjectURL(objectUrlRef.current); }, []);
  useEffect(() => {
    if (!resumeCollectionId) return;
    const controller = new AbortController();
    async function restore() {
      try {
        const response = await predicaoServices.getCollectionToResume(resumeCollectionId!, controller.signal);
        const collection = await readSubmissionResponse(response,
          "A retomada da análise ainda não está disponível nesta API. Atualize o backend para concluir esta coleta com IA.");
        const restored = restoreCollectionForm(collection, resumeCollectionId!);
        if (!controller.signal.aborted) {
          setForm(restored);
          savedIdRef.current = resumeCollectionId;
          setSavedCollectionId(resumeCollectionId);
          setFeedback({ type: "success", message: `Coleta #${resumeCollectionId} recuperada. Você pode concluir a análise com a imagem salva no corpo hídrico ou anexar outra.` });
        }
      } catch (error) {
        if (!controller.signal.aborted) setFeedback({ type: "error", message: error instanceof Error ? error.message : "Não foi possível recuperar a coleta." });
      } finally {
        if (!controller.signal.aborted) setStatus("idle");
      }
    }
    void restore();
    return () => controller.abort();
  }, [resumeCollectionId]);

  function clearError(name: keyof AddAnalysisErrors) {
    setErrors((current) => { const next = { ...current }; delete next[name]; return next; });
  }
  function updateField(name: AddAnalysisFieldName, value: string) {
    if (busyRef.current || savedIdRef.current) return;
    setForm((current) => ({ ...current, [name]: value }));
    clearError(name);
    setFeedback(null);
  }
  function toggleMetal(symbol: MetalSymbol) {
    if (busyRef.current || savedIdRef.current) return;
    setForm((current) => {
      const metals = { ...current.metals };
      if (metals[symbol] === undefined) metals[symbol] = "";
      else delete metals[symbol];
      return { ...current, metals };
    });
    clearError(symbol);
  }
  function updateMetal(symbol: MetalSymbol, value: string) {
    if (busyRef.current || savedIdRef.current) return;
    setForm((current) => ({ ...current, metals: { ...current.metals, [symbol]: value } }));
    clearError(symbol);
  }
  function removeImage() {
    if (busyRef.current || prediction) return;
    if (objectUrlRef.current) URL.revokeObjectURL(objectUrlRef.current);
    objectUrlRef.current = null;
    setImage(null);
    setImagePreviewUrl("");
    clearError("image");
  }
  function updateImage(file: File) {
    if (busyRef.current || prediction) return false;
    const error = validateImage(file);
    if (error) { setErrors((current) => ({ ...current, image: error })); return false; }
    if (objectUrlRef.current) URL.revokeObjectURL(objectUrlRef.current);
    objectUrlRef.current = URL.createObjectURL(file);
    setImagePreviewUrl(objectUrlRef.current);
    setImage(file);
    clearError("image");
    return true;
  }
  function resetForm() {
    if (busyRef.current) return;
    if (objectUrlRef.current) URL.revokeObjectURL(objectUrlRef.current);
    objectUrlRef.current = null;
    setImage(null); setImagePreviewUrl("");
    setForm(initialForm()); setErrors({}); setFeedback(null); setPrediction(null);
    savedIdRef.current = null; setSavedCollectionId(null);
  }
  async function save(withAI: boolean) {
    if (busyRef.current || prediction || (!withAI && savedIdRef.current)) return;
    if (!getAuthSession()) {
      setFeedback({ type: "error", message: "Sua sessão expirou. Entre novamente para continuar." });
      return;
    }
    const nextErrors = validateAnalysis(form);
    setErrors(nextErrors);
    if (Object.keys(nextErrors).length) {
      setFeedback({ type: "error", message: "Revise os campos destacados antes de continuar." });
      requestAnimationFrame(() => document.getElementById(`add-analysis-${Object.keys(nextErrors)[0]}`)?.focus());
      return;
    }
    busyRef.current = true;
    setStatus(withAI ? "analyzing" : "saving");
    setFeedback(null);
    try {
      const payload = buildAnalysisPayload(form);
      const result = await submitAnalysis({ ...payload, savedCollectionId: savedIdRef.current, image: withAI ? image : null, analyzeWithAI: withAI }, {
        createCollection: async (data) => {
          const scope = `coleta:${getAuthSession()!.id}`;
          const key = requestKey(scope, JSON.stringify(data));
          const response = await coletaServices.create(data, key);
          const created = await readSubmissionResponse(response) as { id?: number } | null;
          if (!created || !Number.isInteger(created.id) || created.id! <= 0) throw new Error("O servidor não confirmou o ID da coleta. Consulte os registros antes de repetir o cadastro.");
          clearRequestKey(scope, key);
          return created.id!;
        },
        onCollectionSaved: (id) => { savedIdRef.current = id; setSavedCollectionId(id); },
        analyze: async (id, file) => {
          setStatus("analyzing");
          const scope = `predicao:${getAuthSession()!.id}:${id}`;
          const key = requestKey(scope, await imageFingerprint(file));
          const result = await readSubmissionResponse(await predicaoServices.create(id, file, key));
          const prediction = readPrediction(result, id);
          clearRequestKey(scope, key);
          return prediction;
        },
      });
      setPrediction(result);
      setFeedback({ type: "success", message: result ? "Coleta salva e análise com IA concluída." : "Coleta salva com sucesso. Você pode analisar com IA quando quiser." });
    } catch (error) {
      const detail = error instanceof Error ? error.message : "Não foi possível conectar ao servidor.";
      setFeedback({ type: "error", message: savedIdRef.current ? `A coleta #${savedIdRef.current} está salva, mas a análise com IA não foi concluída. Tente novamente. ${detail}` : detail });
    } finally {
      busyRef.current = false;
      setStatus("idle");
    }
  }
  return { form, errors, image, imagePreviewUrl, updateField, toggleMetal, updateMetal, updateImage, removeImage,
    resetForm, save, savedCollectionId, prediction, status, feedback, isBusy: status !== "idle" };
}
