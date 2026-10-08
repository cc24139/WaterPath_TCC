# Cadastro de análises — implementação inicial

Implementado em 08/10/2026.

## Mudanças na tela

A página mantém a sidebar, os cards, a tipografia, as cores e os componentes de formulário do projeto. O layout continua responsivo, com formulário e resumo/imagem na lateral em telas maiores.

- Dados gerais: corpo hídrico conectado à API, data e horário local da coleta e responsável identificado pela conta conectada.
- Indicadores obrigatórios: temperatura (°C), pH, condutividade elétrica (µS/cm) e oxigênio dissolvido (mg/L).
- Indicadores complementares opcionais: sólidos suspensos totais (mg/L), carbono orgânico total (mg/L) e fósforo total (µg P/L).
- Observações, condição visual, diagnóstico manual e o campo único de metais foram removidos desse formulário.
- Metais pesados: seleção de ferro (Fe), manganês (Mn), cromo (Cr), níquel (Ni), cobre (Cu), zinco (Zn), cádmio (Cd) e chumbo (Pb). Somente os selecionados abrem campos de concentração.
- Fe e Mn usam mg/L; os outros seis usam µg/L. As unidades são fixas no formulário.
- Nenhum valor de exemplo é salvo automaticamente. Os exemplos aparecem apenas nos placeholders.
- O resumo mostra dados gerais, preenchimento dos indicadores, metais, imagem e ID da coleta salva. Os antigos campos invisíveis deixaram de impedir o envio.
- O resultado integrado apresenta nível de risco, motivos e estimativas de metais, separados das medições digitadas.

## Validação

- Aceita vírgula ou ponto decimal e envia números JSON, sem conversão silenciosa de unidades.
- Valida pH entre 0 e 14. Temperatura pode ser negativa; os demais indicadores e metais não podem ser negativos.
- Opcionais vazios são omitidos. Zero continua sendo um valor válido; um metal não selecionado não representa concentração zero.
- Cada metal selecionado precisa de concentração. Desmarcar remove o campo e seu valor do envio.
- Data/hora obrigatória, válida e não futura. O horário local do navegador é convertido para ISO 8601 UTC.
- PNG/JPEG com conteúdo e até 5 MB, alinhado ao texto do componente de upload existente. A API também verifica o conteúdo real da imagem.
- Erros aparecem nos campos e no feedback do envio. O primeiro campo inválido recebe foco.

## Fluxo de integração

1. **Salvar coleta:** envia `POST /api/coleta` com `corpoHidricoId`, `dataHora`, `medicoes` e, se preenchidos, `metaisPesados`.
2. O backend grava coleta, indicadores e metais juntos em um `SaveChanges`. Uma falha de persistência desfaz a operação.
3. **Salvar e analisar com IA:** após confirmar o ID da coleta, envia `POST /api/ia/predicoes` como multipart, com `coletaId`, arquivo `image` e `data` contendo o JSON da amostra.
4. O mesmo mapeamento produz os valores do cadastro e da IA. `metais_pesados` recebe somente os metais medidos, no formato `{ name, value, unit }`.
5. Se a IA falhar, a coleta permanece salva e a repetição usa seu ID, sem recriar a coleta durante essa sessão da tela.
6. Após salvar, os dados medidos ficam bloqueados para manter correspondência com o registro. A imagem pode ser corrigida antes de concluir a IA. “Nova análise” inicia outro cadastro.
7. O usuário pode abrir o monitoramento do corpo hídrico, que consulta novamente as coletas persistidas. A navegação também solicita atualização da rota.

A imagem é enviada apenas ao executar a IA. Salvar somente a coleta não persiste a imagem escolhida. A IA continua exigindo imagem e os quatro indicadores obrigatórios. Esta tela também exige esses indicadores no cadastro sem IA.

## Organização do código

- `constants/addAnalysisOptions.ts`: catálogo de indicadores, códigos de medição, unidades e metais.
- `utils/analysisForm.ts`: validação e montagem de ambos os payloads.
- `utils/submitAnalysis.ts`: sequência de cadastro/análise e tratamento de respostas HTTP.
- `utils/predictionResponse.ts`: validação do resultado antes da apresentação.
- `hooks/useAddAnalysisForm.ts`: estado, upload, feedback e controle de repetição.
- `HeavyMetalsFields`, `AnalysisResult` e `AddAnalysisSummary`: apresentação de cada parte, reutilizando os componentes de UI existentes.
- `api/services/predicaoServices.ts`: envio multipart, usando o `apiFetch` e a sessão existentes.
- DTO de coleta atualizado para o contrato real. Atualização de coleta permanece separada do cadastro de metais; esta entrega não cria uma tela de edição.

## Backend e banco de dados

- `ColetaCadastroInput` estende o contrato de coleta somente para o cadastro, incluindo os metais medidos e suas validações de símbolo, unidade, duplicação e concentração.
- As consultas de coleta retornam os metais persistidos, junto das medições.
- O ID e nome do responsável são capturados das claims do token autenticado. Não são aceitos como identificação livre enviada pelo formulário.
- O responsável é um retrato de quem cadastrou a coleta, não uma certificação de quem realizou a medição em campo.
- O contrato legado de cadastro anônimo continua compatível e grava responsável nulo. Requisições com cabeçalho de autenticação inválido são rejeitadas. A tela exige sessão antes de enviar.
- Medições permitem temperatura negativa e validam o limite superior de pH.
- Concentração de metal passa de `float`/`real` para `double`/`double precision`, evitando perda adicional de precisão em relação aos números enviados à IA. A conversão não recupera precisão perdida em registros antigos.

A migração **`20261008140000_CadastroAnalise`** adiciona `ResponsavelId` e `ResponsavelNome` anuláveis em `Coletas` e amplia a precisão de `MetaisPesados.Concentracao`. Registros antigos não recebem responsáveis inventados. O snapshot e o modelo alvo da migração foram atualizados e verificados por teste.

**A migração não foi aplicada ao banco configurado.** Antes de usar esta tela com persistência, revisar/aplicar a migração no ambiente escolhido e publicar a API atualizada. Com .NET/EF CLI compatível instalado e a conexão do ambiente configurada, a partir de `back-end/api`:

```bash
dotnet ef database update
```

Publicar o backend antes do frontend: uma API antiga pode ignorar a nova lista de metais. A IA deve estar configurada em `IA:BaseUrl` (ou `IA__BaseUrl`) e oferecer o contrato descrito em `back-end/api/CONTRATO_PREDICAO.md`.

## Endereço da API

O frontend agora aceita `NEXT_PUBLIC_API_URL`, incluindo `/api`. Exemplo para desenvolvimento em `frontend/.env.local`:

```dotenv
NEXT_PUBLIC_API_URL=http://localhost:5189/api
```

Sem a variável, permanece o endereço Render que o projeto já utilizava. Reiniciar o servidor de desenvolvimento ou refazer o build ao mudar essa variável. Nenhuma credencial deve ser colocada em uma variável pública do frontend.

## Verificação realizada

- `npm run test:add-analysis`: 10 testes aprovados; inclui decimais, ausências, zero, unidades, datas, seleção parcial, upload, respostas inválidas e repetição da IA sem recriar coleta.
- `npm run test:water-bodies`: 13 testes aprovados.
- `npm run test:dashboard`: 6 testes aprovados.
- TypeScript (`tsc --noEmit`) e ESLint nos arquivos alterados: aprovados.
- `npm run build`: build de produção aprovado.
- `dotnet test back-end/api.Tests/WaterPath.Api.Tests.csproj --no-restore`: 84 aprovados, 1 ignorado. Inclui persistência com responsável, leitura dos metais, precisão, validações, rollback e geração SQL/snapshot da migração.
- O teste opcional de IA real foi ignorado porque `WATERPATH_IA_TEST_URL` e `WATERPATH_TEST_IMAGE` não estavam configurados. Os testes usam SQLite temporário; não houve escrita no banco de produção.
- O backend ainda emite avisos de nulabilidade preexistentes.
- A inspeção visual interativa não foi realizada: o ambiente negou permissão de controle do navegador. A preservação visual foi feita reutilizando componentes, classes e tokens existentes; falta conferir a página renderizada em desktop e celular.

## Implementações futuras

1. **Retomada após recarregar/sair:** consultar uma coleta salva e abrir sua análise pendente. Atualmente o ID usado para repetição fica na memória da página.
2. **Idempotência no servidor:** chave de requisição para cadastro e predição. Cliques concorrentes são bloqueados na tela e a repetição da IA reutiliza a coleta confirmada; perda de conexão após gravação e antes da resposta ainda deixa resultado incerto e pode produzir duplicação em uma nova tentativa.
3. **Comparação temporal de metais:** escolher uma coleta anterior do mesmo corpo hídrico, usar observações medidas comparáveis e preencher `referencia_metais_pesados`. Estimativas não devem substituir medições ausentes.
4. **Consulta dos resultados em outras telas:** conectar “Minhas análises”, histórico e demais telas ainda demonstrativas aos resultados persistidos. O dashboard já consulta coletas, mas a apresentação de todos os novos parâmetros e dos resultados da IA nessas áreas precisa de trabalho próprio.
5. **Edição de coletas:** fluxo explícito para alterar medições/metais e distinguir novas análises dos resultados que usaram valores anteriores.
6. **Resultados abaixo do limite de detecção:** definir como o formulário deve registrar limite e censura, sem transformar “não detectado” em zero. A versão inicial recebe somente concentrações numéricas.
7. **Autoria e acesso:** uniformizar autenticação/autorização dos endpoints legados se todo cadastro precisar obrigatoriamente de responsável, preservando as regras de acesso por corpo hídrico.
8. **Validação em ambiente integrado:** aplicar a migração em desenvolvimento/homologação, testar com a IA real e revisar a interface no navegador, inclusive seleção de todos os metais e telas pequenas.
