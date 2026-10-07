# Contrato de predição e risco atual

A API de IA (`ia/app/DTOs/Amostra.py`, `services/Predict/schema.py`, manifesto `metals_v2` e `services/risk/classification.py`) é a referência do contrato e da regra. Os modelos, pesos, limiares e critérios de classificação foram preservados.

## Origem e uso dos dados

`POST /api/ia/predicoes` recebe `coletaId`, `image` e `data` como multipart. `data` é um objeto JSON textual com **observações da coleta**, enviado pelo cliente. Não é carregado automaticamente de `Medicoes`, `Qualidade` ou `QualidadeFutura`. A API .NET valida a amostra e a coleta, acrescenta o instante UTC da coleta ao campo `data` da amostra e envia a mesma entrada para `/analyze` da IA. Se o cliente informar esse instante, ele precisa coincidir com o da coleta. Coletas sem instante válido ou com instante futuro são rejeitadas nesta análise atual.

O JSON e os valores que seguem para a IA são exatamente os salvos em `EntradaJson`. Não há conversão silenciosa de unidades, strings numéricas ou booleanos. Use as unidades abaixo. A origem desses valores continua sendo o cliente; o vínculo com a coleta não certifica uma medição de laboratório nem estabelece igualdade automática com a tabela `Medicoes`.

| Campo de `data` | Tipo JSON | Unidade | Obrigatoriedade e validação |
| --- | --- | --- | --- |
| `temperatura` | number finito | °C | Obrigatório; o modelo permite temperaturas negativas |
| `ph` | number finito | pH | Obrigatório; 0–14 |
| `condutividade_eletrica` | number finito | µS/cm | Obrigatório; >= 0 |
| `oxigenio_dissolvido` | number finito | mg/L | Obrigatório; >= 0; concentração medida, não saturação em % |
| `solidos_suspensos_totais` | number ou null | mg/L | Opcional; >= 0 quando informado |
| `carbono_organico_total` | number ou null | mg/L | Opcional; >= 0 quando informado |
| `fosforo_total` | number ou null | µg P/L | Opcional; >= 0 quando informado |
| `profundidade` | number ou null | m | Metadado opcional; >= 0; não usado na regressão |
| `estacao`, `latitude`, `longitude`, `estacao_ano` | string ou null | Metadados conforme DTO da IA | Opcionais; não usados na classificação |
| `data` | string ou null na IA | instante da coleta | A .NET preenche em ISO 8601 UTC; não é horizonte futuro |

Opcionais ausentes ou nulos chegam ao imputador existente do modelo como valores ausentes, nunca como zero. Obrigatórios ausentes/nulos, campos desconhecidos ou duplicados, strings numéricas, booleanos, números não finitos ou fora dos limites retornam 422. `nitrogenio_total` está fora do contrato ativo: o manifesto confirma sua remoção das entradas do modelo atual. Sua existência na lista histórica `FIELDS` não o torna entrada aceita.

## Dados que efetivamente classificam o risco

A classificação na IA usa somente:

- `ph` e `oxigenio_dissolvido` da amostra validada;
- sinais visuais dos objetos detectados, pelo método existente `indica_risco()`;
- níveis base da análise integrada mais recente por coleta, em até cinco coletas anteriores do mesmo corpo hídrico.

A regra permanece `waterpath-risk-v1`: presença de sinal visual e/ou pH fora de 6–9 ou OD abaixo de 5 mg/L define o nível base 1–3. Três ou mais alertas no histórico acrescentam um nível, limitado a 3, usando os níveis **base** para evitar realimentação. Temperatura, condutividade e os três opcionais alimentam a regressão de metais; não acrescentam critérios de risco.

A IA retorna `riskInputs: {ph, oxigenio_dissolvido, visualClasses}` com os valores e classes realmente consultados. Cada detecção traz `indicatesRisk`, obtido do mesmo objeto visual. `history.samples` guarda os IDs de predição/coleta e o `baseRiskLevel` usados; `evaluatedCollections`, `alertCollections` e `adjustment` guardam as contagens e o ajuste. A .NET confere tipos, níveis, regra, correspondência entre entrada e `riskInputs`, classes/detecções, totais, histórico enviado e resultado antes de gravar. Uma resposta incompatível retorna 502 e não deixa registro parcial.

`metalPredictions` mantém estimativas de concentração: Fe/Mn em mg/L; Cr/Ni/Cu/Zn/Cd/Pb em µg/L. Não são observações medidas nem previsões para uma data futura. O modelo atual não possui horizonte temporal. `QualidadeFutura` não é consultada neste fluxo.

## Persistência e consulta

| Campo em `waterPath.PredicoesIA` | Conteúdo |
| --- | --- |
| `ColetaId` | FK existente da coleta analisada |
| `CorpoHidricoId` | Cópia do ID do corpo hídrico no momento da análise |
| `DataColeta` | Cópia do instante observado da coleta, UTC (`timestamp with time zone`) |
| `CriadaEm` | Instante de criação da análise, UTC; distinto de `DataColeta` |
| `Tipo` | `integrada` com amostra; `vision` sem amostra |
| `EntradaJson` | Amostra enviada, com seus valores/ausências e referência temporal |
| `ResultadoJson` | Detecções, estimativas, versões dos modelos, `riskInputs`, níveis/motivos/regra e histórico |
| Imagens existentes | Original e anotada, salvas na mesma operação que a entrada e o resultado |

As novas referências são cópias de auditoria, não novas relações com exclusão em cascata. A FK existente da coleta e sua política de exclusão continuam valendo. Editar a coleta não muda a entidade/instante atribuídos à predição salva. As respostas de criação, listagem e detalhe expõem essas referências.

`GET /api/corpohidrico/{id}/risco-atual` usa os dados persistidos, sem chamar a IA, ordenando pelo instante copiado da coleta, ID da coleta, criação/ID da predição. Ignora instantes futuros, análises visuais, entradas incompletas e resultados incompatíveis. Se nenhum resultado válido existir, retorna 404, nunca risco baixo por ausência. Um resultado anterior válido pode ser retornado e sua data é explicitamente informada; não se atribui uma classificação à coleta sem análise.

A migração `20261007120000_ReferenciarDadosClassificacao` acrescenta somente duas colunas anuláveis e preserva todos os registros existentes. Não preenche dados antigos a partir de uma coleta possivelmente editada. Predições legadas sem as referências e sem o retrato verificável ficam disponíveis no detalhe/imagens, mas não entram no risco atual nem na recorrência. Para voltar a utilizá-las, é necessária reanálise com observações confirmadas; não há fabricação de dados nem reclassificação retroativa automática.

## Aplicação e verificação

Publicar a IA com `riskInputs`/`indicatesRisk` antes de atualizar a API .NET. Aplicar a migração no PostgreSQL de destino antes de iniciar a API atualizada. A migração não foi aplicada ao banco configurado durante este trabalho.

Os testes cobrem o multipart entre APIs, inferência real, dados enviados/persistidos, correspondência da classificação, histórico, campos inválidos e ausência de gravação parcial, leitura do risco, referências após edição, legados/futuros, e geração SQL/snapshot da migração sem acessar o PostgreSQL de produção. O teste .NET com IA real usa `WATERPATH_IA_TEST_URL` e `WATERPATH_TEST_IMAGE`.
