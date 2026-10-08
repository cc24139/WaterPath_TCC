// Reaproveita o formato v4 e as rotas das coleções antigas em bin/Debug/net9.0,
// substituindo credenciais, IDs fixos e contratos antigos por fluxos encadeados.
const fs = require('node:fs');
const path = require('node:path');
const root = 'wrk_waterpath_rios_v1';
const resources = [{ _id: root, _type: 'workspace', name: 'WaterPath — Rios sintéticos v1',
  scope: 'collection', description: 'Execute em ordem numérica, somente em desenvolvimento/testes. Veja testing/README.md.' },
{ _id: 'env_waterpath_rios_base', _type: 'environment', parentId: root, name: 'Base Environment',
  data: { base_url: 'http://localhost:5189', token: '', seed_email: 'rios.sinteticos.1@waterpath.invalid',
    seed_password: '', confirm_dev_database: false, user_id: 0,
    seed_river_id: 0, seed_no_risk_id: 0, seed_collection_id: 0, seed_prediction_id: 0,
    seed_river_name: '[SINTETICO v1] Rio Aurora', seed_river_name_url: '',
    run_nonce: '', created_river_id: 0, created_collection_id: 0, created_measurement_id: 0,
    created_user_id: 0, created_river_name: '', created_river_name_url: '', owned_run: '',
    missing_id: -2147483000, missing_positive_id: 2147483000, period_start: '2026-09-01T00:00:00Z', period_end: '2026-09-07T00:00:00Z' } }];
const safety = `if (insomnia.environment.get('confirm_dev_database') !== true) throw new Error('Confirme o banco de desenvolvimento/testes em confirm_dev_database.');
const base = insomnia.environment.get('base_url');
if (!/^https?:\\/\\/(localhost|127\\.0\\.0\\.1)(:\\d+)?$/.test(base)) throw new Error('Esta coleção aceita somente API local.');`;
const helpers = `function check(name, actual, expected) { insomnia.test(name, () => insomnia.expect(actual).to.eql(expected)); }
function body() { return insomnia.response.json(); }
function text() { return insomnia.response.text(); }
const status = insomnia.response.status;`;
const test = (code, extra = '') => `${helpers}\ncheck('HTTP ${code}', status, ${code});\nif (status === ${code}) { ${extra} }`;
const msg = (code, text) => test(code, `check('Mensagem da resposta', text().includes(${JSON.stringify(text)}), true);`);
const riverShape = `const b = body(); check('ID positivo', b.id > 0, true); check('Nome e localização', typeof b.nome === 'string' && typeof b.localizacao === 'string', true); check('Tamanho positivo', b.tamanho > 0, true); check('Usuários sem senha', Array.isArray(b.users) && b.users.every(u => !('senha' in u)), true);`;
let folder, index = 0, order = 0;
function group(id, name) {
  folder = `fld_rios_${id}`; order = 0;
  resources.push({ _id: folder, _type: 'request_group', parentId: root, name, environment: {}, metaSortKey: index++ * 1000 });
}
function req(id, name, method, url, { auth = false, body, pre = '', after, description = '' } = {}) {
  resources.push({ _id: `req_rios_${id}`, _type: 'request', parentId: folder, name, method,
    url: `{{ _.base_url }}${url}`, description, metaSortKey: order++ * 100,
    body: body === undefined ? {} : { mimeType: 'application/json', text: JSON.stringify(body, null, 2) },
    parameters: [], headers: body === undefined ? [] : [{ name: 'Content-Type', value: 'application/json' }],
    authentication: auth ? { type: 'bearer', token: '{{ _.token }}', prefix: 'Bearer' } : {},
    preRequestScript: `${safety}\n${pre}`, afterResponseScript: after || test(200),
    settingFollowRedirects: 'off', settingDisableRenderRequestBody: false });
}
const credentials = `const password = insomnia.environment.get('seed_password');
if (!password) throw new Error('Configure seed_password no ambiente privado.');`;
const raw = expression => `insomnia.request.body.update({ mode: 'raw', raw: JSON.stringify(${expression}) });`;
const capture = (variable, field) => `if (status === 200) insomnia.environment.set('${variable}', body().${field});`;

group('auth', '00 — Autenticação');
req('login', '01 Login do usuário da população', 'POST', '/api/user/login', { body: {}, pre: `${credentials}
insomnia.environment.set('token', ''); insomnia.environment.set('user_id', 0);
${raw("{ email: insomnia.environment.get('seed_email'), senha: password }")}`,
after: test(200, `if (status === 200) { const b = body(); check('Token e usuário', typeof b.token === 'string' && b.token.length > 20 && b.id > 0, true); insomnia.environment.set('token', b.token); insomnia.environment.set('user_id', b.id); }`) });
req('login_invalid', '02 Login com senha incorreta', 'POST', '/api/user/login', { body: {},
pre: raw("{ email: insomnia.environment.get('seed_email'), senha: 'invalid-' + Date.now() + Math.random() }"),
after: test(401, `check('Motivo de autenticação', body().mensagem.includes('inválidos'), true);`) });
req('login_missing', '03 Login sem senha', 'POST', '/api/user/login', { body: { email: '{{ _.seed_email }}' },
after: test(400, `check('ProblemDetails de validação', body().status === 400 && typeof body().errors === 'object', true);`) });

group('fixtures', '01 — Consultar a população');
req('list', '01 Listar rios e capturar IDs sintéticos', 'GET', '/api/corpohidrico', { after: test(200, `
const list = body(); check('Lista', Array.isArray(list), true);
const names = ['Rio Aurora', 'Rio das Pedras Azuis', 'Rio Horizonte', 'Rio Recorrência', 'Rio Fronteira', 'Rio Sem Análise'];
for (let i = 0; i < names.length; i++) {
  const matches = list.filter(x => x.nome === '[SINTETICO v1] ' + names[i]);
  check('Fixture única: ' + names[i], matches.length, 1);
  if (matches.length === 1) insomnia.environment.set('seed_river_' + (i + 1) + '_id', matches[0].id);
}
insomnia.environment.set('seed_river_id', insomnia.environment.get('seed_river_1_id'));
insomnia.environment.set('seed_no_risk_id', insomnia.environment.get('seed_river_6_id'));
insomnia.environment.set('seed_river_name_url', encodeURIComponent(insomnia.environment.get('seed_river_name')));`) });
req('seed_id', '02 Consultar por ID', 'GET', '/api/corpohidrico/{{ _.seed_river_id }}', { auth: true, after: test(200, riverShape + ` check('Rio selecionado', b.nome, insomnia.environment.get('seed_river_name'));`) });
req('seed_name', '03 Consultar por nome exato', 'GET', '/api/corpohidrico/nome/{{ _.seed_river_name_url }}', { after: test(200, riverShape) });
req('by_user', '04 Consultar rios do usuário autenticado', 'GET', '/api/corpohidrico/usuario', { auth: true,
after: test(200, `check('Vínculos do usuário', body().length >= 6 && body().every(r => r.users.some(u => u.id === insomnia.environment.get('user_id'))), true);`) });
req('user_name', '05 Consultar rota usuario/nome', 'GET', '/api/corpohidrico/usuario/{{ _.seed_river_name_url }}', { auth: true,
after: test(200, riverShape), description: 'Esta rota busca por nome sem filtrar pelo usuário; observação do contrato atual.' });
req('seed_collections', '06 Coletas do rio e captura de ID', 'GET', '/api/coleta/corpo-hidrico/{{ _.seed_river_id }}', {
after: test(200, `const list = body(); check('Seis coletas com oito medições', list.length, 6); check('Vínculo e medições', list.every(c => c.corpoHidricoId === insomnia.environment.get('seed_river_id') && c.medicoes.length === 8), true); if (list.length) insomnia.environment.set('seed_collection_id', list.sort((a,b) => a.dataHora.localeCompare(b.dataHora)).at(-1).id);`) });
req('seed_measures', '07 Medições incluindo valor censurado', 'GET', '/api/medicoes/{{ _.seed_collection_id }}', { after: test(200,
`const list = body(); check('Oito medições', list.length, 8); check('Censurada sem valor e com limite', list.some(m => m.censurado && m.valor == null && m.limite > 0), true);`) });
req('seed_period', '08 Coletas por período', 'GET', '/api/coleta/periodo/{{ _.seed_river_id }}?dataInicio={{ _.period_start }}&dataFim={{ _.period_end }}', { auth: true, after: test(200, `check('Seis coletas no período', body().length, 6);`) });
for (const [i, level] of [[1,1],[2,3],[3,3],[4,2],[5,1]]) {
req(`risk_${i}`, `0${8+i} Risco atual do cenário ${i}`, 'GET', `/api/corpohidrico/{{ _.seed_river_${i}_id }}/risco-atual`, { auth: true,
after: test(200, `const b = body(); check('Nível do cenário', b.nivelRisco, ${level}); check('Vínculos e versão', b.corpoHidricoId === insomnia.environment.get('seed_river_${i}_id') && b.coletaId > 0 && b.predicaoId > 0 && b.versaoRegraRisco === 'waterpath-risk-v1', true); check('Motivo sintético', Array.isArray(b.motivos) && b.motivos.some(x => x.includes('SINTETICO')), true); ${i === 1 ? "insomnia.environment.set('seed_prediction_id', b.predicaoId);" : ''}`) }); }
req('risk_empty', '14 Rio sem análise válida', 'GET', '/api/corpohidrico/{{ _.seed_no_risk_id }}/risco-atual', { auth: true, after: msg(404, 'sem resultado de risco válido') });

group('crud', '02 — Criar e alterar registros próprios');
req('create_river', '01 Cadastrar rio descartável', 'POST', '/api/corpohidrico', { auth: true, body: {}, pre: `
if (insomnia.environment.get('created_river_id') > 0 || insomnia.environment.get('owned_run')) throw new Error('Há um rio pendente: execute a limpeza antes de iniciar outra execução.');
const nonce = Date.now().toString(36) + '-' + Math.random().toString(36).slice(2);
const name = '[SINTETICO TESTE] Rio Insomnia ' + nonce;
insomnia.environment.set('run_nonce', nonce); insomnia.environment.set('created_river_name', name);
insomnia.environment.set('created_river_name_url', encodeURIComponent(name));
insomnia.environment.set('created_collection_id', 0); insomnia.environment.set('created_measurement_id', 0);
${raw("{ nome: name, localizacao: '[SINTETICO TESTE] local ' + nonce, tamanho: 7.25, ehPrivado: false, userIds: [] }")}`,
after: msg(200, 'cadastrado com sucesso') + `\nif (status === 200) insomnia.environment.set('owned_run', insomnia.environment.get('run_nonce'));`,
description: 'POST retorna mensagem (200), sem ID/Location. A consulta seguinte captura o ID por nome único.' });
req('capture_river', '02 Consultar nome único e capturar ID', 'GET', '/api/corpohidrico/usuario/{{ _.created_river_name_url }}', { auth: true, after: test(200, `
const b = body(); const nonce = insomnia.environment.get('run_nonce');
const owned = b.nome === insomnia.environment.get('created_river_name') && b.localizacao === '[SINTETICO TESTE] local ' + nonce && b.users.some(u => u.id === insomnia.environment.get('user_id')) && insomnia.environment.get('owned_run') === nonce;
check('Registro criado nesta execução', owned, true);
if (owned) insomnia.environment.set('created_river_id', b.id);`) });
const own = `const id = insomnia.environment.get('created_river_id');
if (!(id > 0) || insomnia.environment.get('owned_run') !== insomnia.environment.get('run_nonce')) throw new Error('Sem ID pertencente à execução atual.');`;
req('update_river', '03 PUT válido — divergência conhecida', 'PUT', '/api/corpohidrico/{{ _.created_river_id }}', { auth: true, body: {}, pre: `${own}\n${raw("{ nome: insomnia.environment.get('created_river_name'), localizacao: '[SINTETICO TESTE] local ' + insomnia.environment.get('run_nonce'), tamanho: 8.5, ehPrivado: false }")}`,
after: msg(200, 'atualizado com sucesso'), description: 'Espera sucesso, mas o handler atual perde o ID e responde 400. A falha é intencional para expor a divergência.' });
req('create_collection', '04 Cadastrar coleta própria com medições', 'POST', '/api/coleta', { body: {}, pre: `${own}\n${raw("{ corpoHidricoId: id, dataHora: '2026-09-07T12:00:00Z', latitude: -23.5, longitude: -46.6, profundidadeMetros: 1, medicoes: [{codigoMedicao:'Temperatura',valor:22,unidade:'°C'},{codigoMedicao:'Ph',valor:7.2,unidade:'pH'},{codigoMedicao:'OxigenioDissolvido',valor:6,unidade:'mg/L'},{codigoMedicao:'CondutividadeEletrica',valor:120,unidade:'µS/cm'}] }")}`,
after: test(201, `const b = body(); check('Coleta vinculada', b.id > 0 && b.corpoHidricoId === insomnia.environment.get('created_river_id') && b.medicoes.length === 4, true); if (status === 201 && b.corpoHidricoId === insomnia.environment.get('created_river_id')) insomnia.environment.set('created_collection_id', b.id);`) });
req('get_collection', '05 Consultar coleta própria', 'GET', '/api/coleta/{{ _.created_collection_id }}', { pre: own,
after: test(200, `check('Medições da coleta', body().medicoes.length, 4); check('Rio da coleta', body().corpoHidricoId, insomnia.environment.get('created_river_id'));`) });
req('create_measure', '06 Adicionar medição de turbidez', 'POST', '/api/medicoes', { body: {}, pre: `${own}\n${raw("{ coletaId: insomnia.environment.get('created_collection_id'), medicao: {codigoMedicao:'Turbidez', valor:14, unidade:'NTU'} }")}`, after: msg(200, 'criada com sucesso') });
req('capture_measure', '07 Capturar ID da medição criada', 'GET', '/api/medicoes/{{ _.created_collection_id }}', { pre: own,
after: test(200, `const list = body(); check('Cinco medições', list.length, 5); const m = list.find(m => m.codigoMedicao === 3); check('Turbidez salva', !!m && m.valor === 14 && m.unidade === 'NTU', true); if (m) insomnia.environment.set('created_measurement_id', m.id);`) });
req('get_measure', '08 Consultar medição por ID', 'GET', '/api/medicoes/{{ _.created_collection_id }}/{{ _.created_measurement_id }}', { pre: own, after: test(200, `check('Valor salvo', body().valor, 14);`) });
req('update_measure', '09 Atualizar medição própria', 'PUT', '/api/medicoes', { body: {}, pre: `${own}\n${raw("{ coletaId: insomnia.environment.get('created_collection_id'), medicaoId: insomnia.environment.get('created_measurement_id'), medicao:{codigoMedicao:'Turbidez',valor:15,unidade:'NTU'} }")}`, after: msg(200, 'atualizada com sucesso') });
req('verify_measure', '10 Verificar atualização da medição', 'GET', '/api/medicoes/{{ _.created_collection_id }}/{{ _.created_measurement_id }}', { pre: own, after: test(200, `check('Novo valor persistido', body().valor, 15);`) });
req('duplicate_measure', '11 Medição duplicada', 'POST', '/api/medicoes', { body: {}, pre: `${own}\n${raw("{ coletaId: insomnia.environment.get('created_collection_id'), medicao:{codigoMedicao:'Turbidez',valor:14,unidade:'NTU'} }")}`, after: msg(409, 'medição') });

group('invalid', '03 — Validação e inexistentes');
for (const [id, name, payload, message] of [
  ['size','Tamanho negativo',{nome:'[SINTETICO TESTE] inválido',localizacao:'Local sintético',tamanho:-1},'maior que zero'],
  ['zero','Tamanho zero',{nome:'[SINTETICO TESTE] inválido',localizacao:'Local sintético',tamanho:0},'maior que zero'],
  ['no_name','Nome ausente',{localizacao:'Local sintético',tamanho:1},'nome'],
  ['no_place','Localização ausente',{nome:'[SINTETICO TESTE] inválido',tamanho:1},'localização'],
  ['no_size','Tamanho ausente',{nome:'[SINTETICO TESTE] inválido',localizacao:'Local sintético'},'maior que zero']
]) req(`invalid_${id}`, name, 'POST', '/api/corpohidrico', {auth:true,body:payload,after:msg(400,message)});
req('bad_type','Tamanho com tipo inválido','POST','/api/corpohidrico',{auth:true,body:{nome:'Inválido',localizacao:'Sintética',tamanho:'abc'},after:test(400,`check('Erros de binding', typeof body().errors, 'object');`)});
req('missing_river','Rio inexistente por ID','GET','/api/corpohidrico/{{ _.missing_id }}',{auth:true,after:msg(404,'não encontrado')});
req('missing_name','Rio inexistente por nome','GET','/api/corpohidrico/nome/SINTETICO-INEXISTENTE-{{ _.missing_id }}',{after:msg(404,'não encontrado')});
req('missing_update','PUT de rio inexistente','PUT','/api/corpohidrico/{{ _.missing_id }}',{auth:true,body:{nome:'Sintético',localizacao:'Sintética',tamanho:1},after:msg(400,'não encontrado')});
req('missing_risk','Risco de ID inválido','GET','/api/corpohidrico/{{ _.missing_id }}/risco-atual',{auth:true,after:msg(400,'ID')});
req('missing_positive_risk','Risco de registro inexistente','GET','/api/corpohidrico/{{ _.missing_positive_id }}/risco-atual',{auth:true,after:msg(404,'não encontrado')});
req('missing_collection','Coleta inexistente','GET','/api/coleta/{{ _.missing_id }}',{after:msg(404,'não encontrada')});
req('invalid_collection','Coleta sem corpo hídrico obrigatório','POST','/api/coleta',{body:{dataHora:'2026-09-07T12:00:00Z'},after:msg(400,'Id inválido')});
req('invalid_measure','Medição negativa','POST','/api/medicoes',{body:{coletaId:'{{ _.created_collection_id }}',medicao:{codigoMedicao:'Ph',valor:-1,unidade:'pH'}},pre:raw("{ coletaId: insomnia.environment.get('created_collection_id'), medicao: {codigoMedicao:'Ph',valor:-1,unidade:'pH'} }"),after:msg(400,'não negativo')});
req('missing_unit','Medição sem unidade','POST','/api/medicoes',{body:{},pre:raw("{ coletaId: insomnia.environment.get('created_collection_id'), medicao: {codigoMedicao:'Cor',valor:1} }"),after:test(400,`check('Erro presente', text().length > 0, true);`)});
req('missing_measure','Medição inexistente','GET','/api/medicoes/{{ _.seed_collection_id }}/{{ _.missing_id }}',{after:msg(400,'inválido')});
req('risk_zero','Risco ID zero','GET','/api/corpohidrico/0/risco-atual',{auth:true,after:msg(400,'ID')});
req('all_measures','Listar todas as medições — conferir roteamento','GET','/api/medicoes',{after:test(200,`check('Lista de medições', Array.isArray(body()), true);`)});

group('noauth', '04 — Ausência de autenticação nas rotas protegidas');
for (const [id,method,url,payload] of [
 ['create','POST','/api/corpohidrico',{nome:'Sintético',localizacao:'Sintética',tamanho:1}],
 ['id','GET','/api/corpohidrico/{{ _.seed_river_id }}'],
 ['user','GET','/api/corpohidrico/usuario'],
 ['user_name','GET','/api/corpohidrico/usuario/{{ _.seed_river_name_url }}'],
 ['update','PUT','/api/corpohidrico/{{ _.missing_id }}',{nome:'Sintético',localizacao:'Sintética',tamanho:1}],
 ['risk','GET','/api/corpohidrico/{{ _.seed_river_id }}/risco-atual'],
 ['period','GET','/api/coleta/periodo/{{ _.seed_river_id }}?dataInicio={{ _.period_start }}&dataFim={{ _.period_end }}'],
 ['date','GET','/api/coleta/data/2026-09-01'],
 ['measure_delete','DELETE','/api/medicoes/{{ _.missing_id }}/{{ _.missing_id }}']
]) req(`noauth_${id}`,`${method} ${url}`,method,url,{body:payload,after:test(401,`check('Sem resposta de sucesso', text().length, 0);`)});

group('cleanup', '05 — Excluir somente registros desta execução');
const verifyOwned = `${own}
const response = await new Promise((resolve,reject) => insomnia.sendRequest({url: insomnia.environment.get('base_url') + '/api/corpohidrico/' + id, method:'GET', header:{Authorization:'Bearer ' + insomnia.environment.get('token')}}, (err,res) => err ? reject(err) : resolve(res)));
const river = response.json();
if (response.code !== 200 || river.nome !== insomnia.environment.get('created_river_name') || river.localizacao !== '[SINTETICO TESTE] local ' + insomnia.environment.get('run_nonce') || !river.users.some(u => u.id === insomnia.environment.get('user_id'))) throw new Error('Limpeza bloqueada: propriedade do rio não comprovada.');`;
const verifyCollection = `${verifyOwned}
const cid = insomnia.environment.get('created_collection_id'); if (!(cid > 0)) throw new Error('Sem coleta própria.');
const responseC = await new Promise((resolve,reject) => insomnia.sendRequest({url: insomnia.environment.get('base_url') + '/api/coleta/' + cid, method:'GET'}, (err,res) => err ? reject(err) : resolve(res)));
if (responseC.code !== 200 || responseC.json().corpoHidricoId !== id) throw new Error('A coleta não pertence ao rio desta execução.');`;
req('delete_measure','01 Excluir medição própria','DELETE','/api/medicoes/{{ _.created_collection_id }}/{{ _.created_measurement_id }}',{auth:true,pre:verifyCollection,after:test(204,`check('Resposta vazia', text().length, 0);`)});
req('deleted_measure','02 Confirmar medição excluída','GET','/api/medicoes/{{ _.created_collection_id }}/{{ _.created_measurement_id }}',{pre:own,after:msg(404,'não encontrada')});
req('delete_collection','03 Excluir coleta própria','DELETE','/api/coleta/{{ _.created_collection_id }}',{pre:verifyCollection,after:msg(200,'deletada com sucesso')});
req('deleted_collection','04 Confirmar coleta excluída','GET','/api/coleta/{{ _.created_collection_id }}',{pre:own,after:msg(404,'não encontrada')});
req('delete_river','05 Excluir rio próprio (rota pública atual)','DELETE','/api/corpohidrico/{{ _.created_river_id }}',{pre:verifyOwned,after:msg(200,'deletado com sucesso'),description:'DELETE está público no contrato atual. A pré-verificação autenticada impede limpar fixtures ou outros rios.'});
req('deleted_river','06 Confirmar rio excluído','GET','/api/corpohidrico/{{ _.created_river_id }}',{auth:true,after:msg(404,'não encontrado') + `\nif (status === 404) { for (const key of ['created_river_id','created_collection_id','created_measurement_id']) insomnia.environment.set(key, 0); insomnia.environment.set('owned_run',''); }`});

group('registration','06 — Cadastro de usuário de teste');
req('register','01 Cadastrar usuário sintético novo','POST','/api/user/cadastro',{body:{},pre:`${credentials}
const email = 'insomnia.' + Date.now().toString(36) + '.' + Math.random().toString(36).slice(2) + '@waterpath.invalid';
insomnia.environment.set('created_user_email',email);
${raw("{nome:'[SINTETICO TESTE] Usuário Insomnia', email, senha:password}")}`,after:test(200,`check('Conta criada', body().id > 0 && body().email === insomnia.environment.get('created_user_email'), true); ${capture('created_user_id','id')}`),description:'A API não tem DELETE de usuário. Este usuário permanece, identificado como sintético.'});
req('register_duplicate','02 Email já cadastrado','POST','/api/user/cadastro',{body:{},pre:`${credentials}\n${raw("{nome:'[SINTETICO TESTE] Duplicado',email:insomnia.environment.get('seed_email'),senha:password}")}`,after:test(400,`check('Email duplicado', body().mensagem.includes('já cadastrado'), true);`)});
req('register_invalid','03 Email inválido','POST','/api/user/cadastro',{body:{},pre:`${credentials}\n${raw("{nome:'[SINTETICO TESTE] Inválido',email:'email-invalido',senha:password}")}`,after:test(400,`check('Erro de email', body().mensagem.includes('inválido'), true);`)});
req('register_missing','04 Cadastro sem senha — divergência conhecida','POST','/api/user/cadastro',{body:{},pre:raw("{nome:'[SINTETICO TESTE] Sem senha',email:'sem-senha.' + Date.now().toString(36) + '.' + Math.random().toString(36).slice(2) + '@waterpath.invalid'}"),after:test(400,`check('Validação automática', body().status === 400 && typeof body().errors === 'object', true);`) + `\nif(status === 200) insomnia.environment.set('missing_password_user_id', body().id);`});

const target = path.join(__dirname, 'insomnia-waterpath-rios.json');
fs.writeFileSync(target, JSON.stringify({_type:'export',__export_format:4,__export_date:'2026-10-08T00:00:00.000Z',__export_source:'waterpath-repository',resources}, null, 2) + '\n');
console.log(`${resources.filter(r => r._type === 'request').length} requisições geradas em ${target}`);
