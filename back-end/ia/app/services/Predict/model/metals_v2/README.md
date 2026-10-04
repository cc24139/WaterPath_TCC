# Predição v2 — preparação e treino no notebook

Toda a implementação de preparação e treinamento está em [MultiOutputRegressor_v2.ipynb](MultiOutputRegressor_v2.ipynb), executado integralmente em 04/10/2026. Não usa scripts locais `.py` para preparar ou treinar o modelo.

A pasta `Predict/model/metals_v2_20261004/` foi escolhida porque é o caminho que o **main.py já carrega**. Assim, dados, notebook, documentação, histórico e modelo ficam em uma única pasta da versão, sem mudar o código da API ou sua configuração. O notebook original [MultiOutputRegressor.ipynb](../../MultiOutputRegressor.ipynb), o main.py, DTO e contrato de entrada foram preservados. `Predict/schema.py` permanece somente como contrato de runtime importado pela API; não é usado pelo treinamento.

## Estrutura

```text
metals_v2_20261004/
  MultiOutputRegressor_v2.ipynb   # preparação, treino, métricas e exportação
  requirements-training.txt
  README.md
  DATASETS_PESQUISADOS.md
  preservation_checks.json      # hashes do main.py e notebook original
  quality_model.joblib          # novo MultiOutputRegressor, usado pela API
  manifest.json                 # arquitetura, unidades, splits e métricas
  example_request.json
  train_samples.csv
  val_samples.csv
  test_samples.csv
  data/
    gemstat_prepared/            # dataset normalizado e metadados da fonte
    external/                   # fontes baixadas, ZIP e CSVs originais
  runs/                         # resultados separados por execução
  historico/                    # Random Forest anterior e primeiro ensaio
```

O ZIP de 201 MB, as execuções e o histórico ficam preservados localmente e fora do Git. O dataset preparado, notebook executado, modelo ativo e seu manifesto podem ser versionados. O CSV normalizado e seu hash permanecem os mesmos da primeira v2: **14.034 amostras, 777 estações, nove países**. A [pesquisa de fontes](DATASETS_PESQUISADOS.md) documenta o GEMStat e as demais bases auditadas.

## Arquitetura baseada no notebook original

Usa **MultiOutputRegressor**, com um regressor por metal. Cada regressor contém:

1. Transformação quantílica do alvo, invertida automaticamente para retornar na unidade original.
2. **StackingRegressor** com Random Forest, XGBoost e CatBoost.
3. **RidgeCV** como combinador final, seguindo o notebook de referência.

Cada modelo base tem sua própria pipeline de imputação. As medianas são ajustadas dentro de cada fold, evitando a imputação global antes do stacking. As previsões usadas para ajustar o combinador são geradas por **GroupKFold de três folds, agrupados por estação**.

Duas configurações regularizadas foram comparadas apenas na validação. Escolhida `stacking_reference`: 100 árvores/iterações por modelo base, RF com profundidade 8 e mínimo de cinco amostras por folha, XGBoost com profundidade 5 e taxa 0,05, CatBoost com profundidade 4 e taxa 0,05. O ajuste final usa treino + validação. Os oito estimadores trabalham sequencialmente para controlar memória e os threads são limitados na inferência.

O notebook tem todas as funções de aquisição, conversão de unidades e preparação nas próprias células. Se o CSV preparado existir, ele é validado pelo hash e reaproveitado; se faltar, o notebook baixa o ZIP e reconstrói a base. Nunca inventa OD ou alvos de metais. Não importa `prepare_dataset.py`, `train_model.py` ou módulos locais de treinamento; esses dois scripts foram removidos.

## Entradas preservadas

| Campo | Unidade | Obrigatório |
| --- | --- | --- |
| temperatura | °C | Sim |
| ph | pH | Sim |
| condutividade_eletrica | µS/cm | Sim |
| oxigenio_dissolvido | mg/L | Sim |
| solidos_suspensos_totais | mg/L | Não |
| carbono_organico_total | mg/L | Não |
| fosforo_total | µg P/L | Não |

O nitrogênio total continua fora das entradas do modelo, preservando o contrato atual. A avaliação anterior de retirada está arquivada no histórico do Random Forest; não é apresentada como nova ablação do stacking. Saturação em % não substitui OD em mg/L. As frações total/dissolvida não são misturadas: esta versão prevê os oito metais totais.

## Resultado do treino executado

| Partição | Amostras | Estações |
| --- | ---: | ---: |
| Treino | 9.361 | 541 |
| Validação | 1.955 | 119 |
| Teste | 1.662 | 117 |

As estações e os identificadores são disjuntos. Com o limite de 120 coletas por estação, o conjunto das partições contém 12.978 registros. O modelo entregue usa 11.316 amostras no ajuste final; nenhuma amostra de teste participa dele.

Erro absoluto médio normalizado por metal, usando desvios padrão do treino:

| Modelo | Erro no mesmo teste |
| --- | ---: |
| Novo MultiOutputRegressor + stacking | 0.130471 |
| Random Forest anterior | 0.138177 |
| Mediana | 0.172625 |

O erro caiu **5.58% em relação ao Random Forest anterior** e **24.42% em relação à mediana**. A comparação usa o mesmo teste já avaliado na primeira v2; é uma comparação exploratória, não uma nova validação independente. A escolha de configuração usa somente a validação.

| Metal | MAE no teste | Unidade | R² |
| --- | ---: | --- | ---: |
| Fe, mg/L | 0.35944 | mg/L | 0.3151 |
| Mn, mg/L | 0.02289 | mg/L | 0.4050 |
| Cr, µg/L | 0.71069 | µg/L | 0.3626 |
| Ni, µg/L | 2.96834 | µg/L | 0.0081 |
| Cu, µg/L | 2.86140 | µg/L | 0.0352 |
| Zn, µg/L | 12.61722 | µg/L | 0.0297 |
| Cd, µg/L | 0.04295 | µg/L | 0.1013 |
| Pb, µg/L | 0.63038 | µg/L | 0.0599 |

O ganho de MAE não significa melhora de todas as métricas: o R² piorou em alguns metais e permanece baixo para Ni, Cu, Zn e Pb. A transformação quantílica evita previsões negativas, mas limita a saída à faixa observada no ajuste e não extrapola novos extremos. Não há estações brasileiras nessa interseção. O modelo estima a amostra atual, não uma data futura.

## Executar novamente

Abra o notebook nesta pasta, selecione um kernel Python 3.13 com as dependências de `requirements-training.txt` e execute as células em ordem. A instalação pode ser feita no terminal:

```powershell
python -m pip install -r requirements-training.txt
```

Cada execução cria uma pasta própria em `runs/`. A última célula copia modelo, manifesto, exemplo e partições para os caminhos que a API já utiliza. Para apenas experimentar, pare antes dessa célula. Reinicie uma API já aberta para limpar o cache do modelo. O Docker existente continua copiando `quality_model.joblib` e `manifest.json`, sem alteração de caminhos ou do estágio YOLO.

## Verificações

- Todas as nove células de código do notebook foram executadas e seus resultados estão salvos.
- **14 testes passaram**, incluindo carga em outro processo, arquitetura MultiOutputRegressor/StackingRegressor, ordem e unidade dos oito alvos, separação por estação/amostra, estatísticas reais das imputações, exigência de OD e requisição multipart da API.
- O SHA-256 do main.py é igual ao registrado antes da reorganização. O notebook original também foi preservado.
- O teste da rota isola a etapa visual; não houve retreino ou alteração dos pesos do YOLO.
- Foi corrigido somente um erro de digitação existente no parâmetro `names` do módulo comum de relatório, que impedia a rota de funcionar.
- Joblib serializado com classes padrão das bibliotecas, sem classes locais do notebook. Dependências verificadas.
