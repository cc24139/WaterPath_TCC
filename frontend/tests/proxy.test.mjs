import assert from "node:assert/strict";
import test from "node:test";
import { readFile } from "node:fs/promises";
import { stripTypeScriptTypes } from "node:module";
import { NextRequest } from "next/server.js";

// Executa o proxy real com NextRequest/NextResponse, sem subir servidor ou simular as regras.
const source = stripTypeScriptTypes(await readFile(new URL("../src/proxy.ts", import.meta.url), "utf8"));
const serverModule = new URL("../node_modules/next/server.js", import.meta.url).href;
const { proxy } = await import(`data:text/javascript;base64,${Buffer.from(source.replace('"next/server"', JSON.stringify(serverModule))).toString("base64")}`);
const origin = "http://localhost:3000";
const token = (exp) => `header.${Buffer.from(JSON.stringify({ exp })).toString("base64url")}.signature`;
const request = (path, cookie) => new NextRequest(`${origin}${path}`, {
  headers: cookie ? { cookie: `token=${cookie}` } : {},
});
const privatePaths = ["", "/dashboard", "/analysis", "/history", "/report", "/insert"].map(path => `/water-bodies/12${path}`);

test("área individual exige login em todas as entradas, incluindo acesso direto", () => {
  for (const path of [...privatePaths, "/water-bodies/12/history/", "/water-bodies/12/futura-funcao"]) {
    const response = proxy(request(path));
    const destination = new URL(response.headers.get("location"));
    assert.equal(response.status, 307);
    assert.equal(destination.origin, origin);
    assert.equal(destination.pathname.replace(/\/$/, ""), "/login");
    assert.equal(destination.searchParams.get("next"), path);
  }
});

test("redirecionamento conserva destino e filtros sem misturá-los aos parâmetros do login", () => {
  const path = "/water-bodies/12/dashboard?period=90&next=%2Foutro&busca=rio%20azul";
  const destination = new URL(proxy(request(path)).headers.get("location"));
  assert.deepEqual([...destination.searchParams.keys()], ["next"]);
  assert.equal(destination.searchParams.get("next"), path);
});

test("listagem, demonstração e demais páginas públicas continuam abertas", () => {
  for (const path of ["/", "/login", "/register", "/about-us", "/methodology", "/water-bodies", "/water-bodies/", "/water-bodies/demo", "/water-bodies/demo/"]) {
    assert.equal(proxy(request(path)).headers.get("x-middleware-next"), "1");
  }
});

test("sessão não expirada permite continuar na área individual", () => {
  for (const path of privatePaths) {
    assert.equal(proxy(request(path, token(Date.now() / 1000 + 3600))).headers.get("x-middleware-next"), "1");
  }
});

test("sessão expirada é removida e leva ao login com retorno ao rio", () => {
  const path = "/water-bodies/12/history?pagina=2";
  const response = proxy(request(path, token(1)));
  assert.equal(new URL(response.headers.get("location")).searchParams.get("next"), path);
  assert.match(response.headers.get("set-cookie"), /token=;/);
  assert.equal(proxy(request("/water-bodies", token(1))).headers.get("x-middleware-next"), "1");
});

test("demais páginas privadas mantêm a proteção e login autenticado retorna à home", () => {
  assert.equal(new URL(proxy(request("/my-analyses")).headers.get("location")).pathname, "/login");
  assert.equal(proxy(request("/login?next=%2Fwater-bodies%2F12", token(Date.now() / 1000 + 3600))).headers.get("location"), `${origin}/`);
});
