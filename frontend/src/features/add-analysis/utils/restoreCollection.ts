import { indicatorFields, initialAddAnalysisForm, metalOptions } from "../constants/addAnalysisOptions.ts";
import type { AddAnalysisFormState, MetalSymbol } from "../types/addAnalysis";
import { validateAnalysis } from "./analysisForm.ts";

function object(value: unknown): Record<string, unknown> {
  if (!value || typeof value !== "object" || Array.isArray(value)) throw new Error("Dados da coleta inválidos.");
  return value as Record<string, unknown>;
}

function localDateTime(value: string): string {
  const date = new Date(value);
  if (!Number.isFinite(date.getTime())) throw new Error("Data da coleta inválida.");
  const pad = (number: number) => String(number).padStart(2, "0");
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

export function restoreCollectionForm(value: unknown, expectedId: number): AddAnalysisFormState {
  const collection = object(value);
  if (collection.id !== expectedId || !Number.isSafeInteger(collection.corpoHidricoId)
    || (collection.corpoHidricoId as number) <= 0 || typeof collection.dataHora !== "string"
    || !Array.isArray(collection.medicoes) || !Array.isArray(collection.metaisPesados))
    throw new Error("A coleta salva não pôde ser lida.");

  const form: AddAnalysisFormState = { ...initialAddAnalysisForm, metals: {},
    waterBody: String(collection.corpoHidricoId), analysisDate: localDateTime(collection.dataHora) };
  for (const field of indicatorFields) {
    const matches = collection.medicoes.map(object).filter(item => item.codigoMedicao === field.numericCode
      || item.codigoMedicao === field.measurementCode);
    if (matches.length > 1) throw new Error(`A coleta contém ${field.label} duplicado.`);
    const measurement = matches[0];
    if (!measurement) continue;
    const expectedUnit = field.unit.replace(/[μµ]/g, "u").toLowerCase();
    const actualUnit = typeof measurement.unidade === "string"
      ? measurement.unidade.replace(/[μµ]/g, "u").toLowerCase() : "";
    if (measurement.censurado || actualUnit !== expectedUnit || typeof measurement.valor !== "number"
      || !Number.isFinite(measurement.valor)) throw new Error(`A medição salva de ${field.label} não pode ser usada nesta análise.`);
    form[field.name] = String(measurement.valor);
  }
  for (const entry of collection.metaisPesados) {
    const metal = object(entry);
    const option = metalOptions.find(item => item.symbol === metal.nome);
    if (!option || form.metals[option.symbol as MetalSymbol] !== undefined || metal.unidade !== option.unit
      || typeof metal.concentracao !== "number" || !Number.isFinite(metal.concentracao))
      throw new Error("Os metais salvos não podem ser usados nesta análise.");
    form.metals[option.symbol] = String(metal.concentracao);
  }
  if (Object.keys(validateAnalysis(form)).length) throw new Error("As medições salvas não atendem aos campos exigidos pela IA.");
  return form;
}
