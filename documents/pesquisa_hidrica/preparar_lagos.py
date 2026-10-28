"""Reproduz a extração das tabelas S1/S5; requer pdfplumber. Não executa inferência."""
from pathlib import Path
import csv
import hashlib
import json
import pdfplumber

ROOT = Path(__file__).resolve().parent
PDF = next((ROOT / 'originais/jianghan').glob('*.pdf'))
LAKES = {'HH1': 'Honghu', 'HH2': 'Honghu', 'LZ': 'Liangzi', 'DY': 'Daye', 'DH1': 'Donghu', 'DH2': 'Donghu'}

def write_csv(path, rows):
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open('w', encoding='utf-8-sig', newline='') as f:
        writer = csv.DictWriter(f, fieldnames=list(rows[0]))
        writer.writeheader()
        writer.writerows(rows)

with pdfplumber.open(PDF) as document:
    s1 = [line.split() for line in document.pages[0].extract_text().splitlines() if line.startswith(('Surface ', 'Bottom '))]
    s5 = [line.split() for line in document.pages[5].extract_text().split('Table S6')[0].splitlines() if line.startswith(('Top ', 'Bottom '))]
assert len(s1) == len(s5) == 12
inputs, observations, samples, requests = [], [], [], []
for index, row in enumerate(s1):
    site = ['HH1', 'HH2', 'LZ', 'DY', 'DH1', 'DH2'][index // 2]
    depth = 'surface' if row[0] == 'Surface' else 'bottom'
    sample_id = f'JIANGHAN-202109-{site}-{depth}'
    v = [float(x.replace('−', '-')) for x in row[1:]]
    assert len(v) == 14
    params = dict(temperatura=v[7], ph=v[10], condutividade_eletrica=v[13], oxigenio_dissolvido=v[8], solidos_suspensos_totais=None, carbono_organico_total=None, nitrogenio_total=v[0], fosforo_total=round(v[4] * 1000, 6))
    inputs.append({'amostra_id': sample_id, **params})
    requests.append({'amostra_id': sample_id, 'parametros': params})
    samples.append(dict(amostra_id=sample_id, corpo_hidrico=LAKES[site], ponto=site, camada=depth, campanha_inicio='2021-09-23', campanha_fim='2021-09-26', data_exata='', latitude='', longitude='', profundidade_m=0.5 if depth == 'surface' else '', referencia_profundidade='abaixo da superficie' if depth == 'surface' else '0.5 m acima do fundo; profundidade absoluta nao publicada', elegivel_entrada=True))
for index, row in enumerate(s5):
    site = ['HH1', 'HH2', 'DY', 'LZ', 'DH1', 'DH2'][index // 2]
    depth = 'surface' if row[0] == 'Top' else 'bottom'
    sample_id = f'JIANGHAN-202109-{site}-{depth}'
    assert len(row) == 12
    for metal, raw in zip(['As','Hg','Fe','Cr','Co','Ni','Cu','Zn','Cd','Pb','Mn'], row[1:]):
        if metal not in ['Fe','Cr','Ni','Cu','Zn','Cd','Pb','Mn']:
            continue
        value = float(raw)
        factor = 0.001 if metal in ['Fe', 'Mn'] else 1
        observations.append(dict(amostra_id=sample_id, metal=metal, valor_fonte=raw, unidade_fonte='µg/L', valor_convertido=round(value * factor, 8), unidade_modelo='mg/L' if factor == 0.001 else 'µg/L', qualificacao='zero_reportado_ambiguo' if value == 0 else 'quantificado', usar_em_metricas=value > 0, fracao='agua_digerida_acida_nao_comprovada_dissolvida', tabela_fonte='S5'))
write_csv(ROOT / 'entrada/parametros_lagos.csv', inputs)
write_csv(ROOT / 'validacao/metais_lagos.csv', observations)
write_csv(ROOT / 'amostras_lagos.csv', samples)
(ROOT / 'entrada/parametros_lagos.json').write_text(json.dumps(requests, ensure_ascii=False, indent=2), encoding='utf-8')
assert len({r['amostra_id'] for r in samples}) == 12
assert {r['amostra_id'] for r in inputs} == {r['amostra_id'] for r in observations}
assert len(observations) == 96
assert all(0 <= r['ph'] <= 14 and r['oxigenio_dissolvido'] >= 0 for r in inputs)
report = dict(amostras=12, lagos=4, alvos=96, alvos_positivos=sum(r['usar_em_metricas'] for r in observations), zeros_reportados=sum(not r['usar_em_metricas'] for r in observations), pareamento='ponto + camada dentro da mesma campanha; ordem LZ/DY corrigida entre tabelas', sha256_pdf=hashlib.sha256(PDF.read_bytes()).hexdigest(), inferencia_executada=False)
(ROOT / 'validacao/conferencia.json').write_text(json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps(report, ensure_ascii=False))
