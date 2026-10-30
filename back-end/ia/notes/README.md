# Notebook demonstrativo do modelo WaterPath

Este diretório reúne o notebook e os dados usados na demonstração do modelo de regressão `metals_v2`.

## Pergunta

Como variam as concentrações de metais estimadas pelo WaterPath quando usamos perfis químicos medianos dos países presentes no GEMStat GFQA v3?

O notebook calcula a mediana das variáveis de entrada em cada país com pelo menos 30 amostras e envia esses perfis ao artefato já treinado. Cada perfil é um cenário sintético representativo, não uma amostra observada específica. País é apenas um agrupamento exploratório: não é entrada do modelo, e diferenças estimadas não indicam efeito causal ou comparação de qualidade entre países.

## Arquivos

- `modelo_metais_waterpath.ipynb`: análise exploratória e inferência reproduzível.
- `data/samples.csv`: dados preparados usados para os perfis químicos.
- `data/dataset_manifest.json`: proveniência, licença, contagens e hash do CSV.
- `data/GEMStat_parameter_metadata.csv` e `data/GEMStat_methods_metadata.csv`: metadados originais dos parâmetros e métodos.
- O notebook carrega o modelo versionado em `../app/models/metals_v2/quality_model.joblib` e confere o SHA-256 registrado em `manifest.json`.

## Execução

Abra o notebook a partir da raiz do repositório ou deste diretório e execute as células em ordem. Use Python 3.13 e as dependências de IA listadas em `../app/requirements.txt` (incluindo pandas, NumPy, scikit-learn, XGBoost, CatBoost e joblib). Os dados usados já estão em `data/`; o notebook não baixa arquivos.

Fontes e método completo:

- [GEMStat GFQA v3 no Zenodo](https://doi.org/10.5281/zenodo.18459694) — CC BY 4.0.
- [Notebook de treinamento v2](../training/notebooks/regression/MultiOutputRegressor_v2.ipynb).
- [Relatório e métricas do modelo](../training/experiment_outputs/regression/metals_v2/README.md).
- [Documentação de treinamento](../training/README.md).

## Limites

O modelo estima concentrações para uma amostra atual, não valores futuros. A comparação neste notebook é uma demonstração de cenários de entrada agregados; não é validação do modelo, análise causal ou previsão de surtos. A base tem nove países e não inclui estações brasileiras no recorte usado no treinamento. Valores abaixo do limite de detecção foram aproximados por LOD/2 na preparação, e o ajuste/avaliação e demais ressalvas estão documentados no relatório do experimento. O pacote local do Rio Doce não é usado porque não tem as quatro entradas obrigatórias do modelo.
