# Cadastro de análises — implementação inicial

Implementado inicialmente em 08/10/2026. As seções datadas abaixo registram o estado de cada entrega; o estado atual está nesta seção.

## Atualização — 24/10/2026

- A busca de corpos hídricos lê o IQA importado nas medições de coleta (código `17`, unidade `pontos`, escala de 0 a 100), com a data da coleta. Os registros legados de `Qualidade`, sem data, continuam disponíveis. Um valor sem data não substitui um valor datado.
- Em **Minhas análises**, uma coleta própria sem predição integrada oferece **Concluir análise com IA**. A rota de cadastro recupera as medições e metais persistidos, bloqueia a edição desses dados e permite anexar ou trocar a imagem antes da análise. Uma resposta inexistente, de outra conta ou já analisada não é restaurada.
- O frontend envia `Idempotency-Key` em UUID no cadastro e na análise. A chave pendente é preservada no armazenamento local para repetir a mesma requisição após uma resposta perdida. A API registra chave e hash do conteúdo; repetição idêntica devolve o mesmo registro, e reutilização com conteúdo diferente devolve conflito (`409`). O banco garante unicidade por índice.
- O cadastro de coleta exige autenticação e aceita rios públicos ou rios privados vinculados à conta. A atualização ou exclusão do rio continua restrita aos usuários vinculados. Uma coleta com responsável só pode ser alterada ou excluída por ele; nas coletas antigas sem responsável, vale o vínculo ao corpo hídrico. O cadastro anônimo mencionado nas seções históricas não é mais aceito.
- A migração `20261024120000_AnalysisIdempotency` adiciona chaves e hashes opcionais a `Coletas` e `PredicoesIA`. As colunas opcionais preservam os registros existentes. Aplicar essa migração **antes** de publicar a API e o frontend novos. Não houve implantação remota nesta entrega.
- Verificação local: 117 testes .NET aprovados (2 de IA real ignorados por falta de serviço configurado), 12 testes do cadastro e 17 da busca aprovados, TypeScript e build de produção aprovados. O teste de migração gera o SQL PostgreSQL e confere os índices únicos e o snapshot do EF; a migração nova ainda não foi aplicada a um PostgreSQL real.
- Permanecem pendentes a revisão visual em desktop e celular, a execução da nova migração em PostgreSQL de homologação e a publicação. Os itens históricos de retomada, idempotência e autorização marcados como futuros nas seções abaixo foram atendidos nesta atualização.

### Retomada na API publicada

Em 24/10/2026, `GET /api/ia/predicoes/resumos` na API padrão do Render respondeu `401` sem autenticação, enquanto `GET /api/ia/predicoes/coletas/1/retomada` respondeu `404` com corpo vazio. A rota de retomada existe no código local e exige autenticação; esse contraste indica que a versão publicada ainda não a inclui. O frontend agora informa que é necessário atualizar a API quando recebe esse `404` vazio, preservando a mensagem específica do servidor para uma coleta inexistente ou de outra conta. Para o botão **Concluir análise com IA** funcionar nesse ambiente, aplicar a migração `20261024120000_AnalysisIdempotency` e publicar o backend atualizado antes de publicar o frontend. Não usar a consulta pública legada de coleta como substituta da rota autorizada de retomada.

### Rios disponíveis para análise

O campo `EhPrivado` já existia no modelo, mas não era retornado por `CorpoHidricoDTO`; por isso o formulário só mostrava rios vinculados ao usuário. O DTO agora expõe `ehPrivado`. Um usuário autenticado pode cadastrar coleta e analisar um rio público sem vínculo prévio; um rio privado exige vínculo. A permissão para editar ou excluir rios não foi ampliada. O frontend trata uma resposta antiga sem `ehPrivado` como privada para não presumir acesso público. A API atualizada precisa ser publicada para que os rios públicos apareçam no formulário conectado ao ambiente remoto.

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
3. **Salvar e analisar com IA:** após confirmar o ID da coleta, envia `POST /api/ia/predicoes` como multipart, com `coletaId` obrigatório e `image` opcional. Sem imagem no formulário, a API usa primeiro uma imagem vinculada à coleta e depois a mais recente do corpo hídrico.
4. A API principal monta a amostra e o histórico a partir da coleta e das coletas anteriores do mesmo corpo hídrico. O cliente não envia `amostra`, `data` ou histórico.
5. Se a IA falhar, a coleta permanece salva e a repetição usa seu ID, sem recriar a coleta durante essa sessão da tela.
6. Após salvar, os dados medidos ficam bloqueados para manter correspondência com o registro. A imagem pode ser corrigida antes de concluir a IA. “Nova análise” inicia outro cadastro.
7. O usuário pode abrir o monitoramento do corpo hídrico, que consulta novamente as coletas persistidas. A navegação também solicita atualização da rota.

A imagem selecionada é enviada apenas ao executar a IA. Salvar somente a coleta não persiste a imagem escolhida. A análise exige os quatro indicadores obrigatórios; se faltar algum, a API responde `422`. Esta tela também exige esses indicadores no cadastro sem IA.

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

**Em 08/10, a migração não havia sido aplicada a um banco real. Em 22/10, ela foi aplicada e validada em PostgreSQL local descartável**, junto das demais migrações do projeto, conforme a seção de validação integrada abaixo. O banco remoto/de produção não foi alterado. Antes da publicação, revisar/aplicar as migrações no ambiente escolhido e publicar a API atualizada. Com .NET/EF CLI compatível instalado e a conexão do ambiente configurada, a partir de `back-end/api`:

```bash
dotnet ef database update
```

Publicar o backend antes do frontend: uma API antiga pode ignorar a nova lista de metais. A IA deve estar configurada em `IA:BaseUrl` (ou `IA__BaseUrl`) e oferecer o contrato descrito em `back-end/contracts/ia/README.md`.

## Endereço da API

O frontend agora aceita `NEXT_PUBLIC_API_URL`, incluindo `/api`. Exemplo para desenvolvimento em `frontend/.env.local`:

```dotenv
NEXT_PUBLIC_API_URL=http://localhost:5189/api
```

Sem a variável, permanece o endereço Render que o projeto já utilizava. Reiniciar o servidor de desenvolvimento ou refazer o build ao mudar essa variável. Nenhuma credencial deve ser colocada em uma variável pública do frontend.

## Verificação inicial — 08/10/2026

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
4. **Consulta dos resultados em outras telas:** “Minhas análises”, histórico e o risco no dashboard foram conectados aos resultados persistidos na etapa descrita em **Nível de risco nas telas**. Ainda faltam detalhes completos de cada análise nessas áreas, incluindo todas as medições, estimativas e imagens, além das demais telas demonstrativas.
5. **Edição de coletas:** fluxo explícito para alterar medições/metais e distinguir novas análises dos resultados que usaram valores anteriores.
6. **Resultados abaixo do limite de detecção:** definir como o formulário deve registrar limite e censura, sem transformar “não detectado” em zero. A versão inicial recebe somente concentrações numéricas.
7. **Autoria e acesso:** uniformizar autenticação/autorização dos endpoints legados se todo cadastro precisar obrigatoriamente de responsável, preservando as regras de acesso por corpo hídrico.
8. **Validação no ambiente de publicação e revisão visual:** banco local e IA real foram validados em 22/10, conforme abaixo. Ainda falta repetir a implantação em homologação e revisar a interação no navegador, inclusive seleção de todos os metais e telas pequenas.

## Validação integrada — 22/10/2026

### Escopo e ambiente utilizado

Foi executada a primeira etapa de validação do cadastro: configuração local, migrações reais, persistência dos parâmetros e metais, inferência com imagem, indisponibilidade da IA e repetição da análise. Os testes HTTP usam as funções reais do frontend `buildAnalysisPayload`, `submitAnalysis`, `readSubmissionResponse` e `readPrediction`, a API .NET iniciada pelo `Program.cs`, PostgreSQL e os modelos Python do repositório. Não substituem a interação visual no navegador.

- PostgreSQL **17.11**, compilado a partir do pacote oficial com SHA-256 conferido, em `/tmp/waterpath-validation-20261022`. Banco novo `waterpath_validation`, acessível somente em loopback, porta `15432`.
- API principal em `127.0.0.1:15189`; segunda instância em `127.0.0.1:15190`, com o mesmo banco e JWT, mas `IA__BaseUrl` apontando para uma porta local sem serviço. Isso permite provocar a falha sem interromper a IA usada pelos outros testes.
- IA real em `127.0.0.1:18000`, Python **3.12.15**, modelos `models/vision/best.onnx` e `models/metals_v2/quality_model.joblib` existentes no projeto. Foram mantidas as versões de `requirements.txt`; o XGBoost CPU 3.4.1 foi compilado sem OpenMP apenas neste ambiente temporário, pois o macOS não tinha `libomp` disponível.
- Frontend isolado em `127.0.0.1:13000`, com `NEXT_PUBLIC_API_URL=http://127.0.0.1:15189/api`. Uma cópia temporária preservou o servidor de desenvolvimento que já estava aberto no projeto.
- Credenciais sintéticas e chave JWT temporária, sem gravá-las no repositório. Nenhum banco remoto foi utilizado. A foto veio do conjunto de imagens de treinamento já presente no projeto.

Os serviços temporários foram encerrados após a validação. Logs e evidências da execução ficaram em `/tmp/waterpath-validation-20261022`; essa pasta é temporária. Os resultados e o roteiro reproduzível permanecem documentados no repositório.

### Correções e arquivos alterados nesta etapa

1. **Cliente HTTP da IA:** `back-end/api/src/IA/Application/IaClient.cs` passou a ter um único construtor, com logger opcional. A aplicação real retornava HTTP 500 porque `AddHttpClient<IaClient>` encontrava dois construtores compatíveis. Os testes anteriores construíam o cliente diretamente e não detectavam essa falha. Foi acrescentado o teste `ClienteIaPodeSerResolvidoPeloRegistroHttpDaAplicacao` em `back-end/api.Tests/PredicaoIATests.cs`.
2. **Consulta de datas:** `back-end/api/src/QualidadeFutura/Application/PrevisaoQualidadeFuturaService.cs` captura o instante UTC antes da expressão LINQ. Em `back-end/api.Tests/PrevisaoQualidadeFuturaTests.cs`, o contexto SQLite converte `DateTimeOffset` para `DateTime` UTC, seguindo os demais testes. Corrige as duas falhas encontradas na suíte inicial, sem mudar o esquema PostgreSQL.
3. **Dependências de teste da IA:** `back-end/ia/app/requirements-test.txt` inclui `pytest`, já importado pelos testes de previsão geral, mas ausente da lista de instalação.
4. **Roteiro de integração reutilizável:** `frontend/tests/addAnalysis.integration.mjs`, disponível por `npm run test:add-analysis:integration` em `frontend/package.json`. Cria usuário, corpo hídrico e coletas sintéticos; exige confirmação de banco descartável e endereços em `127.0.0.1`. Pode salvar evidências JSON sem tokens ou senhas.

Não foram alterados os componentes visuais, as cores, a tipografia ou o layout nesta etapa.

### Migrações e persistência verificadas

Um utilitário temporário usou o próprio `WaterPathDbContext` com Npgsql/EF para executar `Database.MigrateAsync()`. Antes havia **8 migrações pendentes**; depois, **8 aplicadas e nenhuma pendente**:

```text
20260416123752_InitialCreatePostgres
20260604195930_alterCodigo
20260606212213_relacionamentoCorpoHidricoUser
20260916143143_NomeDaSuaMigracao
20261004181139_PersistirPredicoesIA
20261007120000_ReferenciarDadosClassificacao
20261008140000_CadastroAnalise
20261017021850_EvoluirQualidadeFuturaPredicaoGeral
```

Esta comprovação cobre a criação de um banco vazio até o esquema atual. Não comprova a migração de uma base antiga com dados reais; essa conferência continua necessária em homologação.

| Cenário integrado | Resultado comprovado |
| --- | --- |
| Acesso HTTP à página | Sem sessão: 307 para `/login`. Com cookie da sessão real: 200 e HTML contendo condutividade elétrica e metais pesados. |
| Salvar sem metais e sem opcionais | 201; consulta posterior retornou quatro medições, nenhum metal e responsável identificado pela sessão. A operação não chamou a IA. |
| Seleção parcial e inferência | Fe `0,123456789 mg/L` e Pb `0 µg/L` persistidos exatamente; somente dois metais medidos. IA retornou 201, oito estimativas e risco válido aceito pelo leitor do frontend. |
| Imagens e consulta do resultado | Predição consultável pelo ID; imagem original e imagem anotada recuperadas com 200, tipo de imagem e conteúdo não vazio. |
| Todos os campos | Sete indicadores e oito metais recuperados, com as unidades previstas e zeros preservados nos metais. |
| IA indisponível e repetição | Falha 503, coleta preservada e nenhuma predição criada. Repetição com o ID confirmado retornou 201; uma única criação de coleta e uma predição, sem aumento da quantidade de coletas na repetição. |

### Resultados das suítes

| Verificação | Resultado em 22/10 |
| --- | --- |
| Frontend: cadastro / corpos hídricos / dashboard | **32 aprovados**: 10 + 15 + 7. |
| Build de produção do frontend | **Aprovado**, incluindo TypeScript. Precisou de acesso à rede para as fontes já usadas pelo projeto. |
| Backend .NET com IA real habilitada | **113 aprovados, nenhum ignorado**; inclui os dois testes opcionais com IA real e o novo teste do registro HTTP. |
| IA Python (`pytest`) | **53 testes e 77 subtestes aprovados**. |
| Roteiro HTTP integrado | **5 grupos de cenários aprovados**, incluindo a resposta HTTP do frontend. |
| Sintaxe e ESLint do novo roteiro | **Aprovados**. |
| Revisão visual e interação desktop/mobile | **Pendente**: a ferramenta retornou `Computer Use permissions are not granted`. |

Permanecem avisos de nulabilidade do backend, depreciação de HTTPX no cliente de testes FastAPI e de manipulação de arrays no joblib/NumPy. Eles não impediram os testes, mas devem ser acompanhados ao atualizar dependências. Os resultados locais demonstram execução e integração dos modelos; não medem a acurácia científica deles.

### Como repetir a validação

Preparar um PostgreSQL descartável e aplicar as migrações. Iniciar a API com `ConnectionString` desse banco, `JWT_KEY` de teste, `ASPNETCORE_URLS=http://127.0.0.1:15189` e `IA__BaseUrl=http://127.0.0.1:18000`. Iniciar uma segunda instância com a mesma conexão/chave, porta `15190` e IA apontando para `http://127.0.0.1:18001`, que deve ficar sem serviço. Usar `--no-launch-profile` ao executar por `dotnet run`, para preservar as portas configuradas.

Na pasta `back-end/ia/app`, com Python 3.12+ e as dependências de teste instaladas:

```bash
PYTHONPATH=.. python -m pytest tests -q
python -m uvicorn main:app --host 127.0.0.1 --port 18000
```

Para incluir a verificação HTTP da página, iniciar o frontend com `NEXT_PUBLIC_API_URL=http://127.0.0.1:15189/api` na porta `13000`. Em outro terminal, na pasta `frontend`:

```bash
export WATERPATH_INTEGRATION_DISPOSABLE_DB=yes
export WATERPATH_INTEGRATION_API_URL=http://127.0.0.1:15189/api
export WATERPATH_INTEGRATION_OFFLINE_API_URL=http://127.0.0.1:15190/api
export WATERPATH_INTEGRATION_FRONTEND_URL=http://127.0.0.1:13000
export WATERPATH_TEST_IMAGE=/caminho/para/uma/imagem-real.jpg
export WATERPATH_INTEGRATION_REPORT=/tmp/waterpath-integration-result.json
npm run test:add-analysis:integration
```

`WATERPATH_INTEGRATION_FRONTEND_URL` e `WATERPATH_INTEGRATION_REPORT` são opcionais. O roteiro cria novos dados a cada execução e não os apaga; por isso deve usar banco descartável. Para os dois testes reais da suíte .NET, configurar `WATERPATH_IA_TEST_URL=http://127.0.0.1:18000/` e `WATERPATH_TEST_IMAGE` antes de executar `dotnet test back-end/api.Tests/WaterPath.Api.Tests.csproj` a partir da raiz.

### O que ainda falta após esta etapa

1. Liberar o controle do navegador e conferir a página em desktop e celular: abrir/desmarcar metais, mensagens e foco dos erros, seleção de imagem, estado de carregamento, bloqueio após salvar e repetição da IA pela interface. A resposta HTML e os testes das funções não comprovam hidratação, cliques ou layout.
2. Repetir migração e publicação em homologação com cópia representativa dos dados antigos; verificar configuração do serviço de IA no ambiente de destino e disponibilidade das imagens após reinício.
3. Executar no ambiente de publicação a mesma matriz de cenários e acompanhar os avisos de dependências. A configuração usada aqui foi temporária; não foi gravado um `.env` de produção nem realizado deploy.
4. Continuar os itens funcionais da seção **Implementações futuras**, especialmente retomada após recarregar a página e idempotência no servidor. A repetição validada mantém o ID na mesma sessão; não resolve uma resposta perdida após gravação no servidor.

## Nível de risco nas telas — 22/10/2026

### Comportamento implementado

- **Minhas análises:** consulta as coletas cujo responsável é o usuário autenticado. A listagem deixou de usar os exemplos fixos e apresenta corpo hídrico, localização, data da coleta, responsável, análise mais recente, risco e motivos expansíveis. Coletas antigas sem responsável não são atribuídas automaticamente à conta.
- **Histórico do corpo hídrico:** a página antes vazia usa a mesma listagem, restrita ao corpo hídrico e às permissões existentes. Há busca por nome/localização, ordenação pela data da coleta e paginação de dez itens. A busca é enviada ao confirmar o formulário, evitando requisição a cada tecla.
- **Dashboard:** o card de IQA indisponível foi substituído pelo risco da última coleta; os motivos aparecem no diagnóstico e cada linha do histórico recente recebe seu próprio risco. Não é reaproveitada a classificação de uma coleta antiga quando a mais recente não tem resultado. O gráfico continua mostrando medições reais, agora iniciando em pH, sem a opção de IQA vazia.
- **Resultado do cadastro:** usa o mesmo componente de risco das demais telas.
- **Classificações:** 1 = baixo, 2 = moderado, 3 = alto, com texto, ícone e cores já usadas no projeto. “Sem análise de risco” indica coleta sem predição integrada. “Risco indisponível” indica resultado inválido ou consulta indisponível. Ausência não vira zero ou risco baixo.
- Datas da coleta e da análise são identificadas separadamente e apresentadas no horário de Brasília. O resultado exibido é o persistido pela IA; o frontend não calcula IQA nem reclassifica medições.
- Os controles demonstrativos de editar, duplicar e excluir, assim como contagens de processamento/rascunhos fictícias, não são exibidos nessa listagem real. Não há novas ações de escrita nesta etapa.

### Reutilização, contrato e eficiência

- `frontend/src/features/analysis-risk/components/RiskBadge.tsx`: apresentação única do nível de risco.
- `frontend/src/features/analysis-risk/components/AnalysisHistory.tsx`: busca, paginação, estados de carregamento/erro/vazio e listagem compartilhados entre “Minhas análises” e histórico.
- `frontend/src/features/analysis-risk/hooks/useAnalysisSummaries.ts`: consulta autenticada, cancelamento de requisições antigas e nova tentativa. Dados de uma consulta anterior não aparecem enquanto muda usuário, busca, página ou corpo hídrico.
- `frontend/src/features/analysis-risk/utils/analysisRisk.ts`: tipos e validação do contrato; a validação do resultado do cadastro reutiliza a mesma regra dos níveis 1–3.
- `useAuthSession` informa quando a sessão já foi lida, evitando mostrar o aviso de login antes dessa leitura.
- `GET /api/ia/predicoes/resumos`: novo endpoint autenticado, sem alteração de esquema ou migração. Aceita `corpoHidricoId`, `somenteMinhas`, `coletaIds` (até 100), `busca`, `antigasPrimeiro`, `pagina` e `tamanhoPagina` (até 100).
- `AnalysisSummaryQuery.cs` conta e pagina no banco, seleciona a última predição **integrada** por coleta (data de criação e ID como desempate) e lê os resultados da página em lote. São até três consultas por página; não há uma consulta por item, nem carregamento de imagens ou entradas internas.
- A resposta contém `total`, `pagina`, `tamanhoPagina` e `itens`. Cada item inclui identificação da coleta/corpo hídrico, datas, responsável, `predicaoId` e `risco: { level, reasons }`. Os campos anuláveis são explícitos mesmo com a configuração global de omissão de nulos.
- O risco só é exibido quando as referências de corpo hídrico e data da predição correspondem à coleta. Resultado inválido ou legado sem referência permanece indisponível; não há retorno silencioso a uma classificação antiga.
- O dashboard solicita em lote somente os IDs da última coleta e das cinco linhas visíveis. A falha dessa consulta não bloqueia as medições.

### Validação desta entrega

- **36 testes do frontend aprovados**: os 32 anteriores e quatro novos testes de níveis válidos, estados sem resultado, payloads malformados e preservação dos dados.
- **116 testes do backend aprovados com IA real**, incluindo três novos testes de seleção da última predição, paginação/busca, isolamento de usuário, campos nulos e autenticação da rota.
- Build de produção, TypeScript e ESLint dos arquivos alterados: aprovados.
- O roteiro `test:add-analysis:integration` foi ampliado para validar o novo endpoint em PostgreSQL: coleta sem risco, risco persistido, paginação e filtro de IDs em lote. Usa o mesmo leitor de resposta do frontend.
- A revisão visual interativa permanece pendente por falta de permissão de controle do navegador. Os componentes existentes de card, botão, sidebar, fontes e cores foram preservados.

### Publicação e próximos passos

Publicar a API atualizada antes do frontend, pois as telas passam a depender de `/api/ia/predicoes/resumos`. Não é necessário criar campo de IQA ou nova tabela. A implantação remota não foi realizada.

Ainda podem ser implementados: filtros por nível de risco com paginação consistente no servidor; detalhes completos da análise; retomada das coletas sem resultado; e atualização das outras telas demonstrativas. A busca geral de corpos hídricos mantém seu contrato legado de IQA e não foi incluída nesta mudança. O histórico agrupa por coleta e mostra a análise integrada mais recente, não todas as reanálises da mesma coleta.

### Diagnóstico da consulta no ambiente publicado — 22/10/2026

Após o relato de erro na listagem, uma consulta sem credenciais a `https://waterpath-tcc.onrender.com/api/ia/predicoes/resumos` retornou **HTTP 404**. A rota existe no código local, mas não está disponível nesse endereço publicado. Não foram encontrados arquivos `.env`, `.env.local` ou `.env.development` no frontend; sem `NEXT_PUBLIC_API_URL` definido ao iniciar/buildar, o projeto usa essa API do Render. A configuração do processo de frontend aberto pelo usuário não foi inspecionada.

A rota já existente `/api/ia/predicoes/` retornou **HTTP 401** sem credenciais, mostrando que o serviço respondeu e exigiu autenticação, enquanto a nova rota retornou 404.

O hook passou a distinguir HTTP 404 com a mensagem “A listagem de análises ainda não está disponível neste ambiente.” Isso melhora o diagnóstico, mas não publica o endpoint. Para utilizar a listagem, é necessário atualizar a API no Render ou executar o backend atualizado com a configuração do banco desejado e apontar `NEXT_PUBLIC_API_URL` para ele, reiniciando/refazendo o build do frontend. Não houve alteração de filtros, credenciais, conexão de banco ou implantação remota nesta correção.
