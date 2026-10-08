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

export function useAddAnalysisForm() {
  const [form, setForm] = useState(initialAddAnalysisForm);
  const [errors, setErrors] = useState<AddAnalysisErrors>({});
  const [image, setImage] = useState<File | null>(null);
  const [imagePreviewUrl, setImagePreviewUrl] = useState("");
  const objectUrlRef = useRef<string | null>(null);
  const [savedCollectionId, setSavedCollectionId] = useState<number | null>(null);
  const savedIdRef = useRef<number | null>(null);
  const [prediction, setPrediction] = useState<PredicaoDTO | null>(null);
  const [status, setStatus] = useState<"idle" | "saving" | "analyzing">("idle");
  const [feedback, setFeedback] = useState<{ type: "error" | "success"; message: string } | null>(null);
  const busyRef = useRef(false);

  useEffect(() => () => { if (objectUrlRef.current) URL.revokeObjectURL(objectUrlRef.current); }, []);

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
    setForm(initialAddAnalysisForm); setErrors({}); setFeedback(null); setPrediction(null);
    savedIdRef.current = null; setSavedCollectionId(null);
  }
  async function save(withAI: boolean) {
    if (busyRef.current || prediction || (!withAI && savedIdRef.current)) return;
    if (!getAuthSession()) {
      setFeedback({ type: "error", message: "Sua sessão expirou. Entre novamente para continuar." });
      return;
    }
    const nextErrors = validateAnalysis(form);
    if (withAI && !image) nextErrors.image = "Anexe uma imagem para analisar com IA.";
    setErrors(nextErrors);
    if (Object.keys(nextErrors).length) {
      setFeedback({ type: "error", message: "Revise os campos destacados antes de continuar." });
      requestAnimationFrame(() => document.getElementById(`add-analysis-${Object.keys(nextErrors)[0]}`)?.focus());
      return;
    }
    busyRef.current = true;
    setStatus(savedIdRef.current ? "analyzing" : "saving");
    setFeedback(null);
    try {
      const payload = buildAnalysisPayload(form);
      const result = await submitAnalysis({ ...payload, savedCollectionId: savedIdRef.current, image: withAI ? image : null }, {
        createCollection: async (data) => {
          const response = await coletaServices.create(data);
          const created = await readSubmissionResponse(response) as { id?: number } | null;
          if (!created || !Number.isInteger(created.id) || created.id! <= 0) throw new Error("O servidor não confirmou o ID da coleta. Consulte os registros antes de repetir o cadastro.");
          return created.id!;
        },
        onCollectionSaved: (id) => { savedIdRef.current = id; setSavedCollectionId(id); },
        analyze: async (id, file, sample) => {
          setStatus("analyzing");
          const result = await readSubmissionResponse(await predicaoServices.create(id, file, sample));
          return readPrediction(result, id);
        },
      });
      setPrediction(result);
      setFeedback({ type: "success", message: result ? "Coleta salva e análise com IA concluída." : "Coleta salva com sucesso. Você pode anexar uma imagem e analisar com IA." });
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
