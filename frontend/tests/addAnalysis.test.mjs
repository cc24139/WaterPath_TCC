import assert from "node:assert/strict";
import test from "node:test";
import { readPrediction } from "../src/features/add-analysis/utils/predictionResponse.ts";
import { initialAddAnalysisForm } from "../src/features/add-analysis/constants/addAnalysisOptions.ts";
import { buildAnalysisPayload, parseDecimal, validateAnalysis, validateImage } from "../src/features/add-analysis/utils/analysisForm.ts";
import { readSubmissionResponse, submitAnalysis } from "../src/features/add-analysis/utils/submitAnalysis.ts";
import { restoreCollectionForm } from "../src/features/add-analysis/utils/restoreCollection.ts";
import { clearRequestKey, requestKey } from "../src/features/add-analysis/utils/idempotencyKeys.ts";
import { canAnalyzeWaterBody } from "../src/features/add-analysis/utils/waterBodyAccess.ts";

const validForm = () => ({ ...initialAddAnalysisForm, waterBody: "12", analysisDate: "2026-01-02T10:30", temperatura: "24,3", ph: "7", condutividade_eletrica: "100", oxigenio_dissolvido: "6", metals: {} });

test("public rivers and linked private rivers are available for analysis", () => {
  assert.equal(canAnalyzeWaterBody({ ehPrivado: false, users: [] }, "7"), true);
  assert.equal(canAnalyzeWaterBody({ ehPrivado: true, users: [{ id: 7 }] }, "7"), true);
  assert.equal(canAnalyzeWaterBody({ ehPrivado: true, users: [{ id: 8 }] }, "7"), false);
  assert.equal(canAnalyzeWaterBody({ users: [] }, "7"), false);
});

test("blank or malformed decimals never become zero", () => {
  for (const raw of ["", " ", "Infinity", "NaN", "0x10", "1.234,56", "3 mg/L", "1e309"]) assert.equal(parseDecimal(raw), null, raw);
  assert.equal(parseDecimal("0"), 0);
  assert.equal(parseDecimal(" -1,5 "), -1.5);
});

test("required indicators, date and body are validated; zero and negative temperature remain valid", () => {
  assert.equal(Object.keys(validateAnalysis(initialAddAnalysisForm)).length, 6);
  const form = { ...validForm(), temperatura: "-2", ph: "0", condutividade_eletrica: "0", oxigenio_dissolvido: "0" };
  assert.deepEqual(validateAnalysis(form), {});
  for (const patch of [{ ph: "14,1" }, { oxigenio_dissolvido: "-1" }, { carbono_organico_total: "abc" }, { analysisDate: "2026-02-30T12:00" }, { analysisDate: "2999-01-01T10:00" }, { waterBody: "2147483648" }]) assert.ok(Object.keys(validateAnalysis({ ...form, ...patch })).length);
});

test("collection payload maps canonical units and preserves zeros and optional metals", () => {
  const { collection } = buildAnalysisPayload(validForm());
  assert.equal(collection.corpoHidricoId, 12);
  assert.equal(collection.dataHora, new Date("2026-01-02T10:30").toISOString());
  assert.equal(collection.medicoes.length, 4);
  const full = buildAnalysisPayload({ ...validForm(), fosforo_total: "0", metals: { Fe: "0,123456789", Pb: "0" } });
  assert.deepEqual(full.collection.metaisPesados, [{ nome: "Fe", concentracao: 0.123456789, unidade: "mg/L" }, { nome: "Pb", concentracao: 0, unidade: "µg/L" }]);
  assert.equal(full.collection.medicoes.at(-1).unidade, "µg/L");
});

test("selected metals require values, but removing a metal allows saving", () => {
  for (const value of ["", "abc", "-0,1"]) assert.ok(validateAnalysis({ ...validForm(), metals: { Fe: value } }).Fe);
  assert.deepEqual(validateAnalysis({ ...validForm(), metals: {} }), {});
  assert.throws(() => buildAnalysisPayload({ ...validForm(), metals: { Fe: "" } }));
});

test("image upload validates type, content and the displayed 5 MB limit", () => {
  assert.ok(validateImage({ type: "image/svg+xml", size: 10 }));
  assert.ok(validateImage({ type: "image/png", size: 0 }));
  assert.ok(validateImage({ type: "image/jpeg", size: 5 * 1024 * 1024 + 1 }));
  assert.equal(validateImage({ type: "image/jpeg", size: 5 * 1024 * 1024 }), undefined);
});

test("saving without image creates a collection without calling IA", async () => {
  let saved;
  const result = await submitAnalysis({ ...buildAnalysisPayload(validForm()), savedCollectionId: null, image: null }, {
    createCollection: async () => 41,
    onCollectionSaved: (id) => { saved = id; },
    analyze: async () => assert.fail("IA must not run"),
  });
  assert.equal(saved, 41);
  assert.equal(result, null);
});

test("analysis without an uploaded image uses the saved collection ID", async () => {
  let analyzed;
  const result = await submitAnalysis({ ...buildAnalysisPayload(validForm()), savedCollectionId: 41, image: null, analyzeWithAI: true }, {
    createCollection: async () => assert.fail("Saved collection must be reused"),
    onCollectionSaved: id => assert.equal(id, 41),
    analyze: async (id, image) => { analyzed = { id, image }; return { id: 9, coletaId: id }; },
  });
  assert.deepEqual(analyzed, { id: 41, image: null });
  assert.equal(result.id, 9);
});

test("IA retry reuses the persisted collection and exact measured values", async () => {
  const payload = buildAnalysisPayload({ ...validForm(), metals: { Pb: "5,25" } });
  let savedCollectionId = null;
  let creates = 0;
  const operations = {
    createCollection: async () => { creates++; return 42; },
    onCollectionSaved: (id) => { savedCollectionId = id; },
    analyze: async () => { throw new Error("IA unavailable"); },
  };
  const image = new File(["fixture"], "rio.jpg", { type: "image/jpeg" });
  await assert.rejects(submitAnalysis({ ...payload, savedCollectionId, image }, operations), /IA unavailable/);
  assert.equal(savedCollectionId, 42);
  const result = await submitAnalysis({ ...payload, savedCollectionId, image }, {
    ...operations,
    analyze: async (id, file) => {
      assert.equal(id, 42); assert.equal(file, image);
      return { id: 9, coletaId: id };
    },
  });
  assert.equal(creates, 1);
  assert.equal(result.id, 9);
});

test("failed collection creation never calls IA or marks collection saved", async () => {
  await assert.rejects(submitAnalysis({ ...buildAnalysisPayload(validForm()), savedCollectionId: null, image: new File(["x"], "x.jpg") }, {
    createCollection: async () => { throw new Error("Cadastro indisponível"); },
    onCollectionSaved: () => assert.fail("Should not mark saved"),
    analyze: async () => assert.fail("Should not analyze"),
  }), /Cadastro indisponível/);
});

test("API errors show ProblemDetails or readable fallback without server HTML", async () => {
  await assert.rejects(readSubmissionResponse(new Response(JSON.stringify({ detail: "Amostra inválida" }), { status: 422 })), /Amostra inválida/);
  await assert.rejects(readSubmissionResponse(new Response("<html>proxy failure</html>", { status: 502 })), /Não foi possível concluir/);
  await assert.rejects(readSubmissionResponse(new Response("", { status: 401 })), /sessão expirou/);
  await assert.rejects(readSubmissionResponse(new Response("", { status: 404 }), "A retomada não está disponível nesta API."), /retomada não está disponível/);
  await assert.rejects(readSubmissionResponse(new Response("Coleta não encontrada para esta conta.", { status: 404 }), "A retomada não está disponível nesta API."), /Coleta não encontrada/);
});


test("IA response must match collection and contain safe display values", () => {
  const result = { id: 2, coletaId: 1, resultado: { riskLevel: 2, riskLabel: "moderado", riskReasons: ["Sinal visual"], metalPredictions: [{ name: "Fe", value: 0.2, unit: "mg/L" }] } };
  assert.equal(readPrediction(result, 1), result);
  assert.throws(() => readPrediction(result, 99));
  assert.throws(() => readPrediction({ ...result, resultado: { ...result.resultado, metalPredictions: [{ name: "Fe", value: null, unit: "mg/L" }] } }, 1));
  assert.throws(() => readPrediction({ ...result, resultado: { ...result.resultado, riskReasons: [{}] } }, 1));
});

test("saved collection can restore a locked form with measured metals and zero", () => {
  const collection = { id: 42, corpoHidricoId: 12, dataHora: "2026-01-02T13:30:00Z", medicoes: [
    { codigoMedicao: 0, valor: -1, unidade: "°C", censurado: false },
    { codigoMedicao: 1, valor: 7, unidade: "pH", censurado: false },
    { codigoMedicao: 4, valor: 0, unidade: "µS/cm", censurado: false },
    { codigoMedicao: 2, valor: 6, unidade: "mg/L", censurado: false },
  ], metaisPesados: [{ nome: "Pb", concentracao: 0, unidade: "µg/L" }] };
  const form = restoreCollectionForm(collection, 42);
  const { collection: rebuilt } = buildAnalysisPayload(form);
  assert.equal(rebuilt.corpoHidricoId, 12);
  assert.equal(rebuilt.medicoes.find(item => item.codigoMedicao === "CondutividadeEletrica").valor, 0);
  assert.throws(() => restoreCollectionForm(collection, 43));
  assert.throws(() => restoreCollectionForm({ ...collection, medicoes: collection.medicoes.slice(1) }, 42));
});

test("lost response reuses the request key, while changed data receives a new key", () => {
  const values = new Map();
  const previous = globalThis.localStorage;
  globalThis.localStorage = {
    getItem: key => values.get(key) ?? null,
    setItem: (key, value) => values.set(key, value),
    removeItem: key => values.delete(key),
  };
  try {
    const first = requestKey("coleta:7", "payload-a");
    assert.equal(requestKey("coleta:7", "payload-a"), first);
    assert.notEqual(requestKey("coleta:7", "payload-b"), first);
    const second = requestKey("coleta:7", "payload-b");
    clearRequestKey("coleta:7", second);
    assert.notEqual(requestKey("coleta:7", "payload-b"), second);
  } finally {
    globalThis.localStorage = previous;
  }
});
