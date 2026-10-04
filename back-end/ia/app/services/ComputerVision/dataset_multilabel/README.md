# Dataset de presença visual para WaterPath

As fotos são rotuladas pela presença de **lixo**, **drainage connection** e
**urbano**. Várias classes podem estar presentes simultaneamente. Não há caixas,
contornos, recortes de objetos ou imagens sintéticas nesta versão.

O classificador padrão do Ultralytics YOLO aprende uma classe por foto. Para
preservar a saída com várias classes, este dataset fornece três tarefas binárias:
`lixo`, `drainage_connection` e `urbano`, cada uma com `ausente` e `presente`.
Uma aplicação combina as três probabilidades de presença. Isso requer três
modelos; não equivale a um único classificador YOLO com múltiplos rótulos.

## Critérios de anotação

- **lixo:** resíduos sólidos de origem humana visíveis na água ou margem.
  Espuma, plantas, folhas, água escura e turbidez não bastam para marcar presença.
- **drainage_connection:** saída visível de tubo, galeria ou canal lateral que
  descarrega/conecta a um corpo d'água. Um rio canalizado, uma ponte, uma tampa de
  bueiro ou tubulação fechada sem saída visível não bastam. A imagem não permite
  concluir se o efluente contém esgoto ou substâncias químicas.
- **urbano:** contexto construído claramente visível junto ao corpo d'água,
  como edificações, vias e ocupação urbana. Um tubo isolado em área vegetada não
  implica ambiente urbano.
- **incerto:** célula vazia no manifesto; a foto não participa do treino daquela
  classe. Ausência de anotação antiga nunca é convertida automaticamente em zero.

`labels.csv` registra presença, grupo de origem, split e justificativa. Rótulos
foram revisados visualmente pelo assistente e ainda precisam de validação humana
independente, principalmente antes da avaliação final do TCC.

## Organização

Esta ampliação inclui 24 fotos novas selecionadas: 12 com presença de urbano e
12 com presença de drainage connection (uma foto contém ambas as classes).
A versão atual reúne 146 fotos revisadas. Os totais positivos são 35 para lixo,
17 para drainage connection e 78 para urbano; rótulos incertos ficam fora da
tarefa correspondente. Os conjuntos de validação e teste ainda são pequenos.

`yolo_classification_v2` é a versão atual. A pasta `yolo_classification_v1`
preserva uma exportação preliminar com outra divisão dos grupos.

`yolo_classification_v2/<classe>/<train|val|test>/<ausente|presente>/*.jpg`

Cada grupo de origem recebe uma única divisão. Quadros da sequência Sahadara e
fotos da mesma sessão local ficam juntos. A divisão também verifica arquivos e
pixels idênticos entre conjuntos. Essa proteção não identifica automaticamente
todas as fotos do mesmo local ou todas as imagens parecidas.

`yolo_classification_v2/summary.json` contém as contagens efetivas por classe,
os rótulos incertos e as verificações realizadas.

## Fontes

As imagens novas foram obtidas do Wikimedia Commons. `candidates/sources.json`
registra página do arquivo, URL original, autor, licença, data e hash do download.
A busca apenas coleta candidatos: arquivos inadequados permanecem excluídos do
treinamento. Preserve esses créditos e as condições específicas de cada licença
quando compartilhar as imagens.

As fotos herdadas de `dataset/` têm origem local/Roboflow e não tiveram permissão
de redistribuição verificada. Arquivos antigos com marca d'água de banco de imagem
ou evidência insuficiente permanecem fora do treino. Não publique o conjunto
completo sem confirmar os direitos das imagens herdadas.

## Preparar uma nova versão

No diretório `ComputerVision`, com Python e Pillow disponíveis:

```powershell
python prepare_classification_dataset.py --output dataset_multilabel/yolo_classification_v3
```

O exportador preserva a foto inteira e reduz somente imagens maiores que 1600 px.
Ele recusa sobrescrever uma versão existente e bloqueia splits com grupos ou
imagens idênticas compartilhadas.

## Treinar

Use um modelo de **classificação**, por exemplo `yolov8n-cls.pt`, em cada pasta de
classe. O arquivo existente `yolov8n.pt` é de detecção e não serve como substituto.
O argumento `data` aponta para a pasta da tarefa, e não para `data.yaml`.

O arquivo `train_presence_yolo.py` prepara o treino com redimensionamento e
bordas neutras, sem cortar a cena. No ambiente de treinamento com Ultralytics:

```powershell
python train_presence_yolo.py --target all --epochs 50 --device cpu
```

Para treinar só uma classe, use `--target urbano` ou
`--target drainage_connection`. Para uma GPU NVIDIA compatível, use `--device 0`.
O script não foi executado como treinamento nesta etapa; ele requer
`ultralytics`, `torch`, `torchvision` e `Pillow`, ausentes no ambiente de treino
local verificado. As dependências da API existente não foram alteradas.

O YOLO de classificação utiliza recortes por padrão. Para manter o contexto da
foto, use transformações que redimensionem ou adicionem bordas à imagem inteira
durante treino, validação e inferência. A documentação oficial mostra como
personalizar `ClassificationDataset`, `ClassificationTrainer` e
`ClassificationValidator`:

Na inferência dos futuros modelos, aplique também `FullFrameResize(imgsz)` à
imagem e envie o tensor ao classificador; não use o processamento padrão que
recorta o centro. A API de detecção atual precisará de adaptação separada para
consumir as probabilidades dos três modelos.

- https://docs.ultralytics.com/tasks/classify/#custom-transforms
- https://docs.ultralytics.com/datasets/classify/

Ajuste os limiares de presença usando **val**. Use **test** apenas na avaliação
final; não escolha parâmetros pelo resultado do teste. Esta etapa prepara os
dados e não inicia treinamento nem altera a API atual, que ainda espera detecções.
