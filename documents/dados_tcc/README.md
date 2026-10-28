# Dados para apresentação e validação do WaterPath

Pacote com dados públicos reais e rastreáveis. Cada corpo hídrico tem subpastas `originais/`, `entrada/`, `validacao/` e `resultados/`. O manifesto geral é `manifesto.csv`; os vínculos detalhados estão em `Rio_Doce/manifesto.json`.

## Fonte incorporada

- **Rio Doce, estação RDO-15 (Linhares/ES):** PMQQS / Monitoramento Rio Doce. Os arquivos originais já existentes no repositório foram preservados em `Rio_Doce/originais/`: resposta JSON de 110 registros, recorte CSV com 8 linhas selecionadas e metadados do recorte. Oito coletas possuem pH e ferro dissolvido medido, no mesmo registro de estação/data/hora e coordenadas.
- **Entrada:** pH de laboratório em `Rio_Doce/entrada/parametros.csv`.
- **Validação independente:** ferro dissolvido em mg/L em `Rio_Doce/validacao/metais_observados.csv` e em `ComparacoesEsperadas` no Neon, com `ValorPrevisto` nulo. Nenhum metal de referência foi usado como feature.
- **Imagens:** nenhuma imagem do conjunto foi ligada às coletas. Imagens do repositório ou banco não são evidência de metais e não foram chamadas de imagens pareadas.

## Estado da integração Neon

Projeto Neon `WaterPath`, branch `production` (`neondb`). Corpo já existente `Rio Doce` (ID 21); as 8 coletas já existiam (IDs 6–13). Para evitar duplicação, esses registros não foram recriados. Foram inseridas 8 comparações reais de Fe na tabela `ComparacoesEsperadas`, ligadas por coleta e ID estável; operação idempotente por chave `(AmostraId, Parametro)`, sem sobrescrita. IDs 33–40. O `README.md` do corpo traz os vínculos e critérios.

## Limites de uso

O modelo atual de metais requer temperatura, pH, condutividade elétrica e oxigênio dissolvido. Este recorte só traz pH e Fe, então não é elegível para inferência e não permite calcular erros de predição. Não houve retreinamento, previsão nem métricas. `resultados/` documenta essa lacuna. Consulte `Rio_Doce/README.md` antes de usar ou redistribuir os dados; o portal não declara licença aberta explícita para a cópia local, mantenha a atribuição e confirme os termos para redistribuição.
