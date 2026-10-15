# Treinamento dos modelos de IA

Este diretório contém o ciclo de desenvolvimento dos modelos e não faz parte do
pacote de serving (`app/`) nem da imagem Docker de produção.

## Estrutura

```text
training/
  datasets/            # fontes, dados preparados e partições de avaliação
  notebooks/           # exploração, preparação e treino reproduzível
  scripts/             # utilitários executáveis do pipeline de treino
  experiment_outputs/  # métricas, execuções e modelos históricos
  base_models/         # pesos de base usados apenas para treinar
```

Os domínios `computer_vision` e `regression` são mantidos separados dentro de
cada etapa. Os scripts de visão resolvem os caminhos a partir desta raiz, então
podem ser chamados de qualquer diretório de trabalho.

## Artefatos de produção

Somente os artefatos promovidos ficam em `../app/models`:

- `vision/best.onnx` para inferência visual;
- `metals_v2/quality_model.joblib` e o respectivo `manifest.json`.

A promoção é explícita: valide o experimento, copie apenas os arquivos de
runtime para `app/models`, confira o hash registrado no manifesto e execute os
testes do serving. Datasets, notebooks, checkpoints intermediários e resultados
de execução nunca devem ser copiados para `app/`.

Variáveis `YOLO_MODEL_PATH` e `QUALITY_MODEL_PATH` continuam permitindo testar
um artefato fora de `app/models` sem misturá-lo ao pacote de produção.
