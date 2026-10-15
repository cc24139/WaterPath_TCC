# Regressor de metais v2 — 04/10/2026

O novo **`metals_v2_20261004`** está treinado, salvo em joblib e configurado como padrão da API. Prediz as concentrações dos oito metais na amostra atual. Não é uma previsão temporal. Os pesos, treino e inferência do YOLO não foram alterados nesta implementação.

## Dataset efetivamente usado

- Fonte: [UNEP GEMS/Water GFQA v3](https://zenodo.org/records/18459694), DOI **10.5281/zenodo.18459694**, CC BY 4.0. Atribuição: Heinle, Lisniak, Saile e GEMS/Water Data Centre. A pesquisa e outras bases estão em [DATASETS_PESQUISADOS.md](DATASETS_PESQUISADOS.md).
- **14.034 amostras válidas, 777 estações e nove países**, de 1992 a 2023. O corte deriva dos parâmetros necessários, não do total de medições do arquivo global.
- Todos os oito alvos são observações da **fração total**, vinculados por estação, data, hora e profundidade. Frações dissolvidas, extraíveis, suspensas e Cr(VI) são excluídas.
- OD, pH, temperatura e condutividade são medidos. Não foram inventadas concentrações de metais, OD, imagens pareadas ou valores para transformar Fishpond em supervisão.
- São descartados conflitos de duplicação, unidades incompatíveis, resultados integrados e qualidade insuficiente. `<LOD` usa LOD/2 e continua sendo um alvo aproximado.
- **18 registros** com OD acima de 40 mg/L foram excluídos por possível erro de unidade/transcrição, sem converter porcentagens por adivinhação. Esse corte é uma triagem de engenharia documentada.
- A base Lake Onego e os pickles antigos foram preservados. Ela não tem OD e não foi misturada à nova base. O CSV de Kouchibouguac foi baixado e auditado, mas não foi misturado por incerteza da fração e possível sobreposição.

`data/metals_v2_20261004/samples.csv` é o dataset normalizado. Seu manifesto registra hashes, unidades, cobertura de atributos, contagens por país e exclusões. O download completo de 201,3 MB fica localmente em `data/external/gemstat/GFQA_v3.zip`, fora do Git, e pode ser readquirido pelo preparador.

## Entradas da API

| Campo JSON | Unidade | Necessidade |
| --- | --- | --- |
| `temperatura` | °C | Obrigatório |
| `ph` | pH, 0–14 | Obrigatório |
| `condutividade_eletrica` | µS/cm | Obrigatório |
| **`oxigenio_dissolvido`** | **mg/L** | **Obrigatório; não aceita saturação em %** |
| `solidos_suspensos_totais` | mg/L | Opcional, imputação pela mediana |
| `carbono_organico_total` | mg/L | Opcional, imputação pela mediana |
| `fosforo_total` | µg P/L | Opcional, imputação pela mediana |

`nitrogenio_total` foi retirado: teve a menor importância entre os opcionais, e a ablação aumentou o erro de validação apenas **0,88%**, dentro da tolerância de 2% definida para simplificar a coleta. DQO por permanganato/dicromato, DBO5, cor, fosfato, amônia, nitrito, nitrato e profundidade também deixam de ser entradas do contrato v2 por disponibilidade/comparabilidade na expansão. Isso **não demonstra que sejam irrelevantes em qualquer água**; não houve ablação desses atributos ausentes na base escolhida. Profundidade pode continuar como metadado opcional.

OD foi mantido conforme solicitado. Sua contribuição isolada foi pequena: a validação sem OD teve erro 0,16498, frente a 0,16551 com OD e sete entradas. Portanto, não se atribui o ganho global à adição de OD.

O contrato mudou: clientes devem enviar OD medido. Requisições antigas sem OD recebem HTTP 422. Campos antigos fora do contrato são ignorados, sem participar da previsão. O preenchimento de opcionais vem da pipeline salva, calculado apenas sobre as partições de ajuste, e aparece em `imputedFeatures` na resposta.

## Treinamento e resultados

Com limite de 120 coletas distribuídas no histórico por estação, o conjunto usado nas partições ficou com **12.978 amostras**. O dataset completo permanece preservado. Foram comparadas Random Forest e Extra Trees, com e sem transformação logarítmica dos alvos; escolhida **Random Forest com log1p**, 120 árvores, profundidade máxima 16 e mínimo de quatro amostras por folha. A transformação é invertida na própria pipeline e o retorno conserva as unidades originais.

| Partição | Amostras | Estações |
| --- | ---: | ---: |
| Treino | 9.361 | 541 |
| Validação | 1.955 | 119 |
| Teste | 1.662 | 117 |

As estações e identificadores de amostra são disjuntos. Hiperparâmetros e retirada de atributo usam somente a validação. O artefato final foi ajustado em treino + validação, **11.316 amostras**, sem usar o teste. Imputação e normalização dos alvos são ajustadas dentro da pipeline.

A métrica de seleção é a média dos MAEs por metal divididos pelo desvio padrão daquele metal no treino. Isso evita somar diretamente mg/L e µg/L ou deixar um metal dominar por sua escala. **No teste: 0,13818**, frente a **0,17263** da mediana ajustada nas mesmas partições: **19,96% menos erro normalizado**. A referência é `DummyRegressor(strategy="median")`, **não o joblib antigo**. Não existe comparação justa com o modelo antigo porque faltam várias de suas entradas na base externa.

| Metal | MAE no teste | Unidade | R² |
| --- | ---: | --- | ---: |
| Fe | 0,36945 | mg/L | 0,3849 |
| Mn | 0,02750 | mg/L | 0,4853 |
| Cr | 0,72809 | µg/L | 0,4207 |
| Ni | 3,03870 | µg/L | 0,0298 |
| Cu | 3,04274 | µg/L | 0,0504 |
| Zn | 13,12577 | µg/L | 0,0363 |
| Cd | 0,04954 | µg/L | 0,2112 |
| Pb | 0,65716 | µg/L | 0,0840 |

Ni, Cu, Zn e Pb ainda têm R² baixo: o ganho médio não implica boa precisão para todos os metais ou para extremos. Não há observações brasileiras nessa interseção, e esses números não comprovam generalização para o Brasil. A API informa fração total e limitações em `GET /predict/infos`.

## Artefatos e integração

`model/metals_v2_20261004/` contém `quality_model.joblib`, `manifest.json`, `example_request.json` e os três CSVs das partições. O manifesto registra seleção, importância por permutação, ablação, métricas, ranges de treino, versões e SHA-256. O joblib usa apenas classes padrão scikit-learn e operações NumPy, sem classes locais necessárias para desserialização.

`main.py` carrega esse artefato por padrão. `QUALITY_MODEL_PATH` permite apontar para outra versão compatível, acompanhada de `manifest.json`. Antes de usá-lo, a API verifica o hash e a igualdade das entradas/alvos entre modelo e manifesto. `GET /predict/infos` publica campos, unidades, versão e métricas; `POST /predict` mantém `multipart/form-data`, com `data` JSON e `image` arquivo. A resposta inclui versão do regressor, fração e parâmetros imputados, além do relatório visual já existente.

O Docker copia o novo joblib, seu manifesto, o contrato e o módulo de integração. O estágio de exportação e pesos do YOLO foi preservado. Não foi feito deploy nem executado build Docker, pois Docker não está instalado nesta máquina.

## Reproduzir

Usar Python 3.13 e executar da pasta `back-end/ia/app`:

```powershell
python -m pip install -r requirements-training.txt
python -B -X utf8 -m services.Predict.prepare_dataset --output services/Predict/data/metals_v3
python -B -X utf8 -m services.Predict.train_model --dataset services/Predict/data/metals_v3 --model-dir services/Predict/model/metals_v3
```

Os scripts recusam sobrescrever versões existentes. Para servir a reprodução, apontar `QUALITY_MODEL_PATH` para seu joblib e reiniciar a API. Para iniciar a versão entregue, instalar `requirements.txt` e usar o comando de execução já empregado no projeto. O exemplo de JSON pode ser copiado de `model/metals_v2_20261004/example_request.json`.

## Verificações realizadas

**14 testes automatizados passaram**, incluindo inferência real do joblib, ordem/unidade dos oito alvos, separação por estação/amostra, ausência de ajuste no teste, estatísticas reais da imputação, obrigatoriedade/validação de OD, metadados e requisição multipart da API. Também passaram os testes existentes de relatório e divisão de imagens. Nos testes da rota, a etapa visual foi isolada com mock; não se afirma ter revalidado a inferência YOLO. A consistência das dependências foi verificada. Avisos de depreciação das bibliotecas não impediram os testes.
