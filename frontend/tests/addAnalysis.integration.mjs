// Execução manual contra serviços locais e banco descartável. Cria dados de teste.
import assert from "node:assert/strict";
import { randomUUID } from "node:crypto";
import { readFile, writeFile } from "node:fs/promises";
import { initialAddAnalysisForm, metalOptions } from "../src/features/add-analysis/constants/addAnalysisOptions.ts";
import { buildAnalysisPayload } from "../src/features/add-analysis/utils/analysisForm.ts";
import { readPrediction } from "../src/features/add-analysis/utils/predictionResponse.ts";
import { submitAnalysis, readSubmissionResponse } from "../src/features/add-analysis/utils/submitAnalysis.ts";
import { readAnalysisSummaries } from "../src/features/analysis-risk/utils/analysisRisk.ts";

function localApi(name) {
  const url = new URL(process.env[name]);
  assert.equal(url.protocol, "http:");
  assert.equal(url.hostname, "127.0.0.1", `${name} deve apontar para um serviço local de teste`);
  assert.equal(url.pathname.replace(/\/$/, ""), "/api");
  return url.href.replace(/\/$/, "");
}

assert.equal(process.env.WATERPATH_INTEGRATION_DISPOSABLE_DB, "yes", "Confirme o uso de banco descartável");
const api = localApi("WATERPATH_INTEGRATION_API_URL");
const offlineApi = localApi("WATERPATH_INTEGRATION_OFFLINE_API_URL");
assert.notEqual(api, offlineApi, "Use uma segunda instância da API com IA indisponível e o mesmo banco");
const image = new File([await readFile(process.env.WATERPATH_TEST_IMAGE)], "amostra.jpg", { type: "image/jpeg" });
const evidence = [];
let token;

async function request(path, { base = api, body, ...options } = {}) {
  return fetch(`${base}${path}`, {
    ...options,
    headers: { ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(body && !(body instanceof FormData) ? { "Content-Type": "application/json" } : {}) },
    body: body instanceof FormData ? body : body ? JSON.stringify(body) : undefined,
    signal: AbortSignal.timeout(120_000),
  });
}

async function json(path, options) {
  return readSubmissionResponse(await request(path, options));
}

const suffix = randomUUID();
const account = { nome: "Validação local", email: `validation-${suffix}@example.invalid`, senha: randomUUID() };
const user = await json("/user/cadastro", { method: "POST", body: account });
token = (await json("/user/login", { method: "POST", body: account })).token;
assert.ok(token);
if (process.env.WATERPATH_INTEGRATION_FRONTEND_URL) {
  const frontend = new URL(process.env.WATERPATH_INTEGRATION_FRONTEND_URL);
  assert.equal(frontend.protocol, "http:");
  assert.equal(frontend.hostname, "127.0.0.1");
  const pageUrl = new URL("/add-analysis", frontend);
  const anonymous = await fetch(pageUrl, { redirect: "manual" });
  assert.equal(anonymous.status, 307);
  assert.equal(new URL(anonymous.headers.get("location"), frontend).pathname, "/login");
  const authenticated = await fetch(pageUrl, { headers: { Cookie: `token=${token}` }, signal: AbortSignal.timeout(120_000) });
  assert.equal(authenticated.status, 200);
  const html = await authenticated.text();
  assert.match(html, /Condutividade elétrica/);
  assert.match(html, /Metais pesados/i);
  evidence.push({ scenario: "Página via HTTP: sessão ausente redireciona; sessão válida recebe formulário", status: "passed" });
}
const riverName = `Validação integração ${suffix}`;
await json("/corpohidrico", { method: "POST", body: { nome: riverName, localizacao: "Ambiente descartável", tamanho: 1, ehPrivado: true, userIds: [] } });
const river = (await json("/corpohidrico/usuario")).find(item => item.nome === riverName);
assert.ok(river?.id);

function payload(metals = {}, optional = {}) {
  return buildAnalysisPayload({ ...initialAddAnalysisForm, waterBody: String(river.id),
    analysisDate: "2026-10-01T10:00", temperatura: "22,5", ph: "7,2",
    condutividade_eletrica: "378", oxigenio_dissolvido: "8,1", metals, ...optional });
}

async function createCollection(body) {
  const response = await request("/coleta", { method: "POST", body });
  assert.equal(response.status, 201, "Cadastro de coleta");
  return (await response.json()).id;
}

async function analyze(id, file, sample, base = api) {
  const body = new FormData();
  body.set("coletaId", String(id));
  body.set("image", file);
  body.set("amostra", JSON.stringify(sample));
  const response = await request("/ia/predicoes", { method: "POST", body, base });
  return { status: response.status, data: await readSubmissionResponse(response) };
}

const empty = payload();
assert.equal(empty.collection.medicoes.length, 4);
assert.equal(empty.collection.metaisPesados, undefined);
let emptyId;
const savedOnly = await submitAnalysis({ ...empty, savedCollectionId: null, image: null }, {
  createCollection, onCollectionSaved: id => { emptyId = id; },
  analyze: () => assert.fail("Salvar coleta não deve chamar IA"),
});
assert.equal(savedOnly, null);
const savedEmpty = await json(`/coleta/${emptyId}`);
assert.equal(savedEmpty.metaisPesados.length, 0);
assert.equal(savedEmpty.medicoes.length, 4);
assert.equal(savedEmpty.responsavelId, user.id);
assert.equal(savedEmpty.responsavelNome, account.nome);
evidence.push({ scenario: "Sem metais e opcionais vazios; responsável da sessão", collectionId: emptyId, status: "passed" });

const partial = payload({ Fe: "0,123456789", Pb: "0" });
let partialId;
const prediction = await submitAnalysis({ ...partial, savedCollectionId: null, image }, {
  createCollection, onCollectionSaved: id => { partialId = id; },
  analyze: async (id, file, sample) => {
    const response = await analyze(id, file, sample);
    assert.equal(response.status, 201);
    return readPrediction(response.data, id);
  },
});
const savedPartial = await json(`/coleta/${partialId}`);
assert.equal(savedPartial.metaisPesados.length, 2);
assert.equal(savedPartial.metaisPesados.find(m => m.nome === "Fe").concentracao, 0.123456789);
assert.equal(savedPartial.metaisPesados.find(m => m.nome === "Pb").concentracao, 0);
assert.equal(prediction.resultado.metalPredictions.length, 8);
assert.equal((await json(`/ia/predicoes/${prediction.id}`)).id, prediction.id);
for (const path of [prediction.imagemOriginalUrl, prediction.imagemResultadoUrl]) {
  const response = await request(path.replace(/^\/api/, ""));
  assert.equal(response.status, 200);
  assert.match(response.headers.get("content-type"), /^image\//);
  assert.ok((await response.arrayBuffer()).byteLength > 0);
}
evidence.push({ scenario: "Metais parciais, precisão, zero, IA real e imagens persistidas", collectionId: partialId, predictionId: prediction.id, riskLevel: prediction.resultado.riskLevel, status: "passed" });

const full = payload(Object.fromEntries(metalOptions.map(m => [m.symbol, "0"])), {
  solidos_suspensos_totais: "0", carbono_organico_total: "0", fosforo_total: "0",
});
const fullId = await createCollection(full.collection);
const savedFull = await json(`/coleta/${fullId}`);
assert.equal(savedFull.medicoes.length, 7);
assert.equal(savedFull.metaisPesados.length, 8);
for (const metal of savedFull.metaisPesados) {
  assert.equal(metal.concentracao, 0);
  assert.equal(metal.unidade, metalOptions.find(m => m.symbol === metal.nome).unit);
}
evidence.push({ scenario: "Sete indicadores, oito metais, unidades e zeros", collectionId: fullId, status: "passed" });

let retryId;
let creations = 0;
let failureStatus;
const retry = payload();
const operations = {
  createCollection: async body => { creations++; return createCollection(body); },
  onCollectionSaved: id => { retryId = id; },
  analyze: async (id, file, sample) => {
    const body = new FormData();
    body.set("coletaId", String(id)); body.set("image", file); body.set("amostra", JSON.stringify(sample));
    const response = await request("/ia/predicoes", { method: "POST", body, base: offlineApi });
    failureStatus = response.status;
    return readSubmissionResponse(response);
  },
};
await assert.rejects(() => submitAnalysis({ ...retry, savedCollectionId: null, image }, operations), /temporariamente indisponível/);
assert.equal(failureStatus, 503);
assert.ok(retryId);
assert.equal((await json(`/ia/predicoes?coletaId=${retryId}`)).total, 0);
assert.equal((await json(`/coleta/${retryId}`)).id, retryId);
const beforeCount = (await json(`/coleta/corpo-hidrico/${river.id}`)).length;
const retried = await submitAnalysis({ ...retry, savedCollectionId: retryId, image }, {
  ...operations, analyze: async (id, file, sample) => {
    const response = await analyze(id, file, sample);
    assert.equal(response.status, 201);
    return readPrediction(response.data, id);
  },
});
assert.equal(creations, 1);
assert.equal((await json(`/coleta/corpo-hidrico/${river.id}`)).length, beforeCount);
assert.equal((await json(`/ia/predicoes?coletaId=${retryId}`)).total, 1);
evidence.push({ scenario: "IA indisponível retorna 503; repetição reutiliza coleta sem duplicação", collectionId: retryId, predictionId: retried.id, status: "passed" });

const summaries = readAnalysisSummaries(await json(`/ia/predicoes/resumos?somenteMinhas=true&corpoHidricoId=${river.id}`));
assert.equal(summaries.total, 4);
assert.equal(summaries.itens.find(item => item.coletaId === emptyId).risco, null);
assert.equal(summaries.itens.find(item => item.coletaId === partialId).risco.level, prediction.resultado.riskLevel);
assert.equal(summaries.itens.find(item => item.coletaId === retryId).predicaoId, retried.id);
assert.equal((await json(`/ia/predicoes/resumos?somenteMinhas=true&tamanhoPagina=1&pagina=2`)).itens.length, 1);
const batch = await json(`/ia/predicoes/resumos?corpoHidricoId=${river.id}&coletaIds=${emptyId}&coletaIds=${partialId}`);
assert.equal(batch.total, 2);
assert.ok(batch.itens.every(item => !('imagemOriginal' in item) && !('resultadoJson' in item)));
evidence.push({ scenario: "Resumos de risco em PostgreSQL: pendência, classificação persistida, paginação e consulta em lote", status: "passed" });

const result = { executedAt: new Date().toISOString(), transport: "HTTP local; PostgreSQL e IA reais; sem navegador", scenarios: evidence };
if (process.env.WATERPATH_INTEGRATION_REPORT) await writeFile(process.env.WATERPATH_INTEGRATION_REPORT, JSON.stringify(result, null, 2) + "\n");
console.log(JSON.stringify(result, null, 2));
