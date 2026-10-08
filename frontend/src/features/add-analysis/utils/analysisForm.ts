import { indicatorFields, metalOptions } from "../constants/addAnalysisOptions.ts";
import type { AddAnalysisErrors, AddAnalysisFormState } from "../types/addAnalysis";
import type { ColetaCadastroDTO } from "../../../api/dtos/coletaDTO";
import type { AmostraDTO } from "../../../api/dtos/predicaoDTO";

export function parseDecimal(value: string): number | null {
  const normalized = value.trim().replace(",", ".");
  if (!/^[+-]?(?:\d+(?:\.\d*)?|\.\d+)$/.test(normalized)) return null;
  const number = Number(normalized);
  return Number.isFinite(number) ? number : null;
}

export function validateAnalysis(form: AddAnalysisFormState, now = Date.now()): AddAnalysisErrors {
  const errors: AddAnalysisErrors = {};
  if (!/^[1-9]\d*$/.test(form.waterBody) || Number(form.waterBody) > 2147483647) errors.waterBody = "Selecione um corpo hídrico válido.";
  const date = new Date(form.analysisDate);
  // datetime-local uses the browser's local timezone; the API receives ISO UTC.
  if (!/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}$/.test(form.analysisDate) || !Number.isFinite(date.getTime()) || date.getFullYear() < 1900) {
    errors.analysisDate = "Informe a data e o horário da coleta.";
  } else if (date.getTime() > now) {
    errors.analysisDate = "A coleta não pode ter data ou horário futuro.";
  } else {
    const [year, month, day, hour, minute] = form.analysisDate.split(/[-T:]/).map(Number);
    if (date.getFullYear() !== year || date.getMonth() + 1 !== month || date.getDate() !== day || date.getHours() !== hour || date.getMinutes() !== minute) errors.analysisDate = "Informe uma data e um horário válidos.";
  }
  for (const field of indicatorFields) {
    const raw = form[field.name].trim();
    if (!raw && !field.required) continue;
    const value = parseDecimal(raw);
    if (value === null) errors[field.name] = raw ? "Informe um número válido." : "Campo obrigatório.";
    else if (field.name === "ph" && (value < 0 || value > 14)) errors.ph = "O pH deve estar entre 0 e 14.";
    else if (field.name !== "temperatura" && value < 0) errors[field.name] = "O valor deve ser maior ou igual a zero.";
  }
  for (const metal of metalOptions) {
    if (form.metals[metal.symbol] === undefined) continue;
    const value = parseDecimal(form.metals[metal.symbol]!);
    if (value === null || value < 0) errors[metal.symbol] = "Informe uma concentração maior ou igual a zero.";
  }
  return errors;
}

export function validateImage(file: Pick<File, "size" | "type">): string | undefined {
  if (!["image/png", "image/jpeg"].includes(file.type)) return "Envie uma imagem PNG ou JPEG.";
  if (!file.size || file.size > 5 * 1024 * 1024) return "A imagem deve ter conteúdo e no máximo 5 MB.";
}

export function buildAnalysisPayload(form: AddAnalysisFormState): { collection: ColetaCadastroDTO; sample: AmostraDTO } {
  if (Object.keys(validateAnalysis(form)).length) throw new Error("Revise os campos antes de salvar.");
  const sample: AmostraDTO = {};
  const collection: ColetaCadastroDTO = {
    corpoHidricoId: Number(form.waterBody), dataHora: new Date(form.analysisDate).toISOString(), medicoes: [],
  };
  for (const field of indicatorFields) {
    const value = parseDecimal(form[field.name]);
    if (value === null) continue;
    sample[field.name] = value;
    collection.medicoes.push({ codigoMedicao: field.measurementCode, valor: value, unidade: field.unit });
  }
  const metals = metalOptions.flatMap((metal) => {
    const raw = form.metals[metal.symbol];
    return raw === undefined ? [] : [{ name: metal.symbol, value: parseDecimal(raw)!, unit: metal.unit }];
  });
  if (metals.length) {
    sample.metais_pesados = metals;
    collection.metaisPesados = metals.map(({ name, value, unit }) => ({ nome: name, concentracao: value, unidade: unit }));
  }
  return { collection, sample };
}
