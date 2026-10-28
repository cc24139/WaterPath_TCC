# Dados reais para demonstração do WaterPath

## Conjunto incluído

Fonte: Programa de Monitoramento Quali-Quantitativo Sistemático (PMQQS), página de download do [Monitoramento Rio Doce](https://monitoramentoriodoce.org/download-dos-dados/), serviço de dados ArcGIS do programa (URL e filtro registrados em `originais/PMQQS-RDO15-LEIA-ME.json`). A cópia original e o subconjunto previamente extraído no repositório estão preservados em `originais/`. Acessado originalmente em 25/10/2026. O site não explicita licença aberta nos metadados copiados; manter atribuição institucional e confirmar condições de reutilização antes de redistribuir fora do TCC.

- Corpo hídrico: Rio Doce; estação RDO-15, ponte sobre o Rio Doce na BR-101, Linhares/ES; coordenadas do serviço: -19.41011, -40.05896.
- Oito registros de água superficial, de 16/12/2025 a 21/07/2026. A data e hora local vieram do mesmo registro fonte; a derivação conserva o fuso -03:00 e precisão de minuto.
- Entrada preparada: `entrada/parametros.csv`, pH laboratorial (pH), medido no campo `Q_211_pH_L`.
- Referência independente: `validacao/metais_observados.csv`, ferro dissolvido (Fe), mg/L, do campo `Q_107_Fe_D`. O código de status original é preservado; todas as oito linhas usadas têm status 1. Resultados censurados não foram selecionados neste subconjunto.
- Não há imagens pareadas no conjunto. Imagens locais/de treino não foram associadas a estas coletas. Aparência de água não comprova presença nem ausência de metais.

## Contrato, separação e limitações

O identificador estável é estação + data/hora (`source_record_key` no manifesto). pH é parâmetro de entrada aceito pelo contrato da IA; Fe é uma saída medida guardada exclusivamente como referência neste pacote. Não enviar Fe como feature. Embora o banco já contenha o Fe na relação `MetaisPesados` das coletas existentes, este pacote mantém uma cópia explícita em `validacao/` e deve ser usado para comparação independente.

Este conjunto **não permite inferência válida pelo modelo atual de metais**: a amostra do contrato requer temperatura, pH, condutividade elétrica e oxigênio dissolvido. O extrato RDO-15 contém apenas pH e Fe dissolvido. Por isso `ia_model_input_eligible=false`, `resultados/` não contém previsões nem métricas, e nenhuma predição foi inventada ou executada. Não há conversões de unidades. O método analítico específico e o limite de detecção não constam no extrato consultado; não foram presumidos.

As observações de Rio Doce/RDO-15 também aparecem em `script/data/` e já estão cadastradas no Neon. Antes de reaproveitá-las em treino/validação, considerar sobreposição: o conjunto não deve ser tratado como uma avaliação inédita do modelo. Não foi encontrado mapeamento entre esses IDs e uma fonte de treino; verificar os manifests do dataset de treino antes de alegar independência.

## Divisão e reprodução

Não foi criada divisão aleatória treino/teste. A ordem temporal está preservada no manifesto e nos CSVs de referência para impedir uso acidental de observações futuras. São oito observações na estação única; isso é insuficiente para avaliar generalização temporal robusta.

Para reproduzir a preparação, use `originais/pmqqs_rdo15.csv` como subconjunto recebido e `originais/pmqqs_rdo15_raw.json` para consultar o retorno de origem completo (110 registros). Compare `source_record_key` com os arquivos em `script/data/`. Os arquivos `entrada/` e `validacao/` não dependem de acesso à API. O corpo e as coletas correspondentes já existiam no Neon `WaterPath`, branch `production`; IDs existentes: corpo 21, coletas 6–13 em ordem cronológica decrescente inversa (13 = mais antiga, 6 = mais recente). Nenhum registro novo foi inserido por esta preparação, evitando duplicação.

`resultados/` fica vazio até que haja amostras com todas as quatro features requeridas e inferência efetivamente executada. Preencher então com predição por coleta/metal, versão do modelo, unidade e métricas (MAE/RMSE; e contagem de pares). Não calcular métricas com alvos ou predições ausentes.
