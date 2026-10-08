// Verificador HTTP complementar. Executa os scripts gerados; não é o runtime nativo do Insomnia.
const fs = require('node:fs');
const path = require('node:path');
const vm = require('node:vm');
const assert = require('node:assert/strict');
const data = JSON.parse(fs.readFileSync(path.join(__dirname, 'insomnia-waterpath-rios.json'), 'utf8'));
const requests = data.resources.filter(r => r._type === 'request');
const ids = new Set();
for (const resource of data.resources) {
  assert(!ids.has(resource._id), `ID duplicado: ${resource._id}`); ids.add(resource._id);
  if (resource.parentId) assert(data.resources.some(r => r._id === resource.parentId));
}
for (const request of requests) {
  for (const script of [request.preRequestScript, request.afterResponseScript])
    new vm.Script(`(async () => { ${script}\n })()`);
  assert(request.afterResponseScript.includes('insomnia.test'));
  assert(request.preRequestScript.includes('confirm_dev_database'));
}
if (process.argv.includes('--check')) {
  console.log(JSON.stringify({requests: requests.length, scripts: requests.length * 2, structure: 'valid', syntax: 'valid'}));
  process.exit(0);
}
const env = {...data.resources.find(r => r._type === 'environment').data,
  base_url: process.env.WATERPATH_TEST_BASE_URL || 'http://localhost:5189',
  seed_password: process.env.WATERPATH_SEED_PASSWORD || '',
  confirm_dev_database: process.env.WATERPATH_TEST_CONFIRM_DEV_DATABASE === 'true'};
assert(env.confirm_dev_database, 'Confirme WATERPATH_TEST_CONFIRM_DEV_DATABASE=true somente para banco de desenvolvimento/testes.');
assert(env.seed_password, 'Configure WATERPATH_SEED_PASSWORD fora dos arquivos entregues.');
assert(/^https?:\/\/(localhost|127\.0\.0\.1)(:\d+)?$/.test(env.base_url), 'Somente API local.');
const interpolate = value => value.replace(/\{\{\s*_\.([a-zA-Z0-9_]+)\s*\}\}/g, (_, key) => {
  assert(env[key] !== undefined, `Variável ausente: ${key}`); return String(env[key]);
});
const call = async (url, options) => {
  assert(new URL(url).origin === new URL(env.base_url).origin, 'Destino externo bloqueado.');
  const res = await fetch(url, {...options, redirect:'manual', signal:AbortSignal.timeout(30000)});
  const text = await res.text();
  return {code:res.status, status:res.status, text:()=>text, json:()=>JSON.parse(text)};
};
const result = {runtime:'node-complementar', requests:[], assertions:0, failures:[]};
async function main() {
  for (const request of requests) {
    let requestBody;
    const tests = [];
    const insomnia = {
      environment: {get:key=>env[key], set:(key,value)=>{env[key]=value;}},
      request: {body: {update:body=>{requestBody=body.raw;}}},
      sendRequest: (req, callback) => {
        const url = typeof req === 'string' ? req : req.url;
        call(url, {method:req.method || 'GET', headers:req.header || {}}).then(res=>callback(null,res),err=>callback(err));
      },
      test: (name, callback) => { result.assertions++; try { callback(); tests.push({name,pass:true}); }
        catch (error) { tests.push({name,pass:false}); result.failures.push({request:request._id,test:name,message:error.message}); } },
      expect: actual => ({to:{eql:expected=>assert.deepStrictEqual(actual,expected)}})
    };
    const run = source => new vm.Script(`(async()=>{${source}\n})()`).runInNewContext({insomnia, console, Date, Math, encodeURIComponent}, {timeout:10000});
    try {
      await run(request.preRequestScript);
      const headers = Object.fromEntries(request.headers.map(h=>[h.name,interpolate(h.value)]));
      if (request.authentication.type === 'bearer') headers.Authorization = 'Bearer ' + interpolate(request.authentication.token);
      const response = await call(interpolate(request.url), {method:request.method,headers,
        body:requestBody ?? (request.body.text ? interpolate(request.body.text) : undefined)});
      insomnia.response = response;
      await run(request.afterResponseScript);
      result.requests.push({id:request._id,name:request.name,status:response.status,tests});
    } catch (error) {
      // Não registrar corpo, senha, token ou erro bruto da rede.
      result.failures.push({request:request._id,test:'execução',message:error.name});
      result.requests.push({id:request._id,name:request.name,status:null,tests});
    }
  }
  const output = process.env.WATERPATH_TEST_REPORT;
  if (output) fs.writeFileSync(output, JSON.stringify(result,null,2) + '\n');
  console.log(JSON.stringify({runtime:result.runtime,requests:result.requests.length,assertions:result.assertions,failures:result.failures}));
  process.exitCode = result.failures.length ? 1 : 0;
}
main().catch(() => {console.error('Falha no verificador; nenhum segredo foi registrado.'); process.exitCode=1;});
