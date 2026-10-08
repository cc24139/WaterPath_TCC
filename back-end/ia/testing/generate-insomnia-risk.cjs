const fs = require('node:fs');
const path = require('node:path');
const workspace = 'wrk_waterpath_ia_risco';
const resources = [
  {_id:workspace,_type:'workspace',name:'WaterPath IA — Risco e metais',scope:'collection',description:'Somente POST /analyze da IA. Body multipart preenchido. Configure base_url e image_path; não exige token nem banco. Os dados e IDs de histórico são sintéticos.'},
  {_id:'env_waterpath_ia_risco',_type:'environment',parentId:workspace,name:'Base Environment',data:{base_url:'http://localhost:8000',image_path:''}}
];
const sample = {
  estacao:'[SINTETICO] Estação de teste Insomnia',latitude:'-23.5505',longitude:'-46.6333',
  data:'2026-10-08T12:00:00Z',profundidade:1.2,estacao_ano:'primavera',
  temperatura:22,ph:7,condutividade_eletrica:100,oxigenio_dissolvido:6,
  solidos_suspensos_totais:12,carbono_organico_total:2.4,fosforo_total:40
};
const history = [
  {predictionId:101,coletaId:201,baseRiskLevel:2},
  {predictionId:102,coletaId:202,baseRiskLevel:3},
  {predictionId:103,coletaId:203,baseRiskLevel:2}
];
const observed = [{name:'Pb',value:10,unit:'µg/L'},{name:'Cd',value:1,unit:'µg/L'},{name:'Fe',value:0.1,unit:'mg/L'}];
const previous = {data:'2026-10-07T12:00:00Z',metais_pesados:[{name:'Pb',value:5,unit:'µg/L'},{name:'Cd',value:2,unit:'µg/L'},{name:'Fe',value:100,unit:'µg/L'}]};
let folder, groupOrder = 0, order = 0;
function group(id,name) {
  folder = 'fld_ia_risco_' + id; order = 0;
  resources.push({_id:folder,_type:'request_group',parentId:workspace,name,environment:{},metaSortKey:groupOrder++ * 1000});
}
function request(id,name,data=sample,past=[],{expected=200,extra='',omitImage=false,description=''}={}) {
  const assertions = `insomnia.test('HTTP ${expected}', () => insomnia.expect(insomnia.response.status).to.eql(${expected}));
if (insomnia.response.status === ${expected}) {
  const b = insomnia.response.json();
  ${expected === 200 ? `
  const input = ${JSON.stringify(data)};
  const sentHistory = ${JSON.stringify(past)};
  const visual = b.riskInputs.visualClasses.length > 0;
  const measurements = input.ph < 6 || input.ph > 9 || input.oxigenio_dissolvido < 5;
  const basis = 1 + Number(visual) + Number(measurements);
  const alerts = sentHistory.filter(x => x.baseRiskLevel >= 2).length;
  const level = Math.min(3, basis + Number(alerts >= 3));
  insomnia.test('Versão de risco', () => insomnia.expect(b.riskRuleVersion).to.eql('waterpath-risk-v1'));
  insomnia.test('Nível base calculado', () => insomnia.expect(b.baseRiskLevel).to.eql(basis));
  insomnia.test('Nível final com histórico', () => insomnia.expect(b.riskLevel).to.eql(level));
  insomnia.test('Rótulo correspondente', () => insomnia.expect(b.riskLabel).to.eql({1:'baixo',2:'moderado',3:'alto'}[level]));
  insomnia.test('Medições efetivamente usadas', () => insomnia.expect(b.riskInputs.ph === input.ph && b.riskInputs.oxigenio_dissolvido === input.oxigenio_dissolvido).to.eql(true));
  insomnia.test('Histórico recebido', () => insomnia.expect(b.history.evaluatedCollections === sentHistory.length && b.history.alertCollections === alerts && b.history.adjustment === level - basis).to.eql(true));
  insomnia.test('Motivos e imagem anotada', () => insomnia.expect(Array.isArray(b.riskReasons) && b.riskReasons.length > 0 && typeof b.annotatedImage === 'string' && b.annotatedImage.length > 0 && b.annotatedImageContentType === 'image/jpeg').to.eql(true));
  insomnia.test('Predições separadas das observações', () => insomnia.expect(Array.isArray(b.metalPredictions) && typeof b.metalVariation === 'object').to.eql(true));
  ${extra}
  ` : "insomnia.test('Detalhe de validação', () => insomnia.expect(b.detail !== undefined).to.eql(true));"}
}`;
  resources.push({_id:'req_ia_risco_'+id,_type:'request',parentId:folder,name,method:'POST',url:'{{ _.base_url }}/analyze',
    description:description || 'Selecione image como File (JPEG/PNG até 10 MB) ou configure image_path com caminho absoluto. data e history são campos Text contendo JSON; não envie o body como application/json. Dados sintéticos.',
    metaSortKey:order++ * 100,parameters:[],headers:[],authentication:{},
    body:{mimeType:'multipart/form-data',params:[
      ...(!omitImage ? [{id:'pair_'+id+'_image',name:'image',type:'file',fileName:'{{ _.image_path }}',value:'',disabled:false}] : []),
      {id:'pair_'+id+'_data',name:'data',type:'text',value:JSON.stringify(data,null,2),disabled:false},
      {id:'pair_'+id+'_history',name:'history',type:'text',value:JSON.stringify(past,null,2),disabled:false}
    ]},preRequestScript:'',afterResponseScript:assertions,settingFollowRedirects:'off'});
}
group('measurements','01 — Risco por medições e histórico');
request('normal','01 Sem alerta nas medições (imagem pode elevar o risco)');
request('alerts','02 pH e OD em alerta',{...sample,ph:5.5,oxigenio_dissolvido:3.2});
request('boundary','03 Limites sem alerta: pH 9 e OD 5',{...sample,ph:9,oxigenio_dissolvido:5});
request('recurrence','04 Alerta atual + três anteriores: risco alto',{...sample,ph:5.5,oxigenio_dissolvido:3.2},history,
  {extra:"insomnia.test('Recorrência leva ao nível alto', () => insomnia.expect(b.riskLevel).to.eql(3));",description:'IDs 101–103 e 201–203 são fictícios. A IA aceita o histórico fornecido pelo cliente, sem consultar um banco. Com medições em alerta e três níveis base >=2, o risco final é 3 independentemente do sinal visual.'});
group('metals','02 — Variação dos metais observados');
request('metals_complete','01 Pb aumenta, Cd reduz e Fe permanece estável',{...sample,metais_pesados:observed,referencia_metais_pesados:previous},[],{extra:`
  insomnia.test('Comparação completa', () => insomnia.expect(b.metalVariation.status).to.eql('completa'));
  for (const [name, variation, delta] of [['Pb','aumento',5],['Cd','reducao',-1],['Fe','estabilidade',0]]) {
    const metal = b.metalVariation.metals.find(x => x.name === name);
    insomnia.test('Variação de ' + name, () => insomnia.expect(!!metal && metal.variation === variation && metal.delta === delta).to.eql(true));
  }`});
request('metals_partial','02 Comparação parcial: metal ausente e unidade incompatível',{...sample,
  metais_pesados:[...observed,{name:'Cu',value:10,unit:'µg/L'},{name:'Zn',value:4,unit:'mg/kg'}],
  referencia_metais_pesados:{...previous,metais_pesados:[...previous.metais_pesados,{name:'Zn',value:5,unit:'µg/L'}]}},[],{extra:`
  insomnia.test('Comparação parcial', () => insomnia.expect(b.metalVariation.status).to.eql('parcial'));
  for (const [name, reason] of [['Cu','dados_ausentes'],['Zn','unidades_incompativeis']]) {
    const metal = b.metalVariation.metals.find(x => x.name === name);
    insomnia.test('Impedimento de ' + name, () => insomnia.expect(!!metal && metal.variation === 'indeterminada' && metal.delta === null && metal.reason === reason).to.eql(true));
  }`});
request('metals_no_reference','03 Sem referência anterior: variação indeterminada',{...sample,metais_pesados:observed},[],{
  extra:"insomnia.test('Sem referência não implica estabilidade', () => insomnia.expect(b.metalVariation.status === 'indeterminada' && b.metalVariation.metals.every(x => x.variation === 'indeterminada' && x.delta === null)).to.eql(true));"});
request('metals_no_data','04 Sem observações de metais: variação indeterminada',sample,[],{
  extra:"insomnia.test('Sem metais observados', () => insomnia.expect(b.metalVariation.status === 'indeterminada' && b.metalVariation.metals.length === 0).to.eql(true));"});
group('validation','03 — Validação de entradas');
const {oxigenio_dissolvido,...missingOxygen} = sample;
request('missing_oxygen','01 OD obrigatório ausente: 422',missingOxygen,[],{expected:422});
request('invalid_ph','02 pH fora de 0–14: 422',{...sample,ph:15},[],{expected:422});
request('invalid_metal','03 Concentração negativa: 422',{...sample,metais_pesados:[{name:'Pb',value:-1,unit:'µg/L'}]},[],{expected:422});
request('duplicate_history','04 Coleta repetida no histórico: 422',sample,[history[0],{predictionId:104,coletaId:201,baseRiskLevel:2}],{expected:422});
request('missing_image','05 Imagem obrigatória ausente: 422',sample,[],{expected:422,omitImage:true});
const target = path.join(__dirname,'insomnia-ia-risco.json');
fs.writeFileSync(target,JSON.stringify({_type:'export',__export_format:4,__export_date:'2026-10-08T00:00:00.000Z',__export_source:'waterpath-ia-risk',resources},null,2)+'\n');
console.log(`${resources.filter(r=>r._type==='request').length} cenários de POST /analyze gerados.`);
