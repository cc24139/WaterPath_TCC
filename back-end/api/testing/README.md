# Rios sintéticos e testes da API

## Pré-requisitos e configuração

.NET 10 (o projeto efetivo usa `net10.0`), PostgreSQL com as migrations atuais, Node.js 18+ para os verificadores auxiliares e Insomnia com suporte a scripts antes/depois da requisição. Os testes .NET existentes usam SQLite em memória; não são prova de execução no PostgreSQL.

A API carrega `back-end/api/.env`. Variáveis já definidas no processo agora prevalecem sobre esse arquivo. Não versione `.env`, senhas, JWTs, strings de conexão nem exportações de ambientes privados. A conexão continua sendo `ConnectionString`; a chave de autenticação continua sendo `JWT_KEY`. Configure-as localmente para um banco exclusivamente de desenvolvimento/testes. O nome do ambiente ASP.NET sozinho não identifica o uso real de um banco remoto.

Na raiz do repositório:

```powershell
dotnet build back-end/api/back-end.csproj
Set-Location back-end/api
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet run --no-launch-profile --no-build -- --seed-rivers-check
```

Esse último comando só consulta o histórico de migrations. Mostra `Target` (`host/database`) e as migrations pendentes, sem credenciais. A rotina de população não executa migrations e não usa `EnsureCreated`, `EnsureDeleted`, `TRUNCATE` ou exclusões.

Se houver migrations pendentes, revise-as e aplique-as separadamente **no banco de desenvolvimento/testes confirmado**. Há migrations antigas que removem colunas; não aplique toda a cadeia sem verificar o histórico/esquema. Os scripts `Infrastructure/Data/Scripts/AtualizarColetas.sql` e `CreateMedicoes.sql` são ajustes legados, não são seeds nem substituem o histórico EF. Não os execute em um banco já atualizado.

## Executar a população

No terminal anterior, copie o `Target` exibido pelo comando de consulta para `WATERPATH_SEED_TARGET`. Essa confirmação deve corresponder exatamente à conexão efetiva. A rotina aceita somente ambientes `Development`, `Testing` e `Test`.

```powershell
$env:WATERPATH_SEED_TARGET = Read-Host 'host/database do banco de desenvolvimento/testes confirmado'
$seedSecret = Read-Host 'Senha dos usuários sintéticos (mínimo 12 caracteres)' -AsSecureString
$env:WATERPATH_SEED_PASSWORD = [System.Net.NetworkCredential]::new('', $seedSecret).Password
dotnet run --no-launch-profile --no-build -- --seed-rivers
if ($LASTEXITCODE -ne 0) { throw 'População não concluída; confira a mensagem sem divulgar credenciais.' }
dotnet run --no-launch-profile --no-build -- --seed-rivers
```

Use a **mesma senha** em todas as execuções e no ambiente privado do Insomnia. A senha é armazenada no banco somente como hash BCrypt; ela não aparece no manifesto. Uma execução bem-sucedida imprime `Inseridos`, `TotaisDataset`, IDs dos usuários e IDs de cada rio/coleta/predição. A segunda execução deve imprimir todos os contadores de `Inseridos` iguais a zero. Não salve os logs globais de configuração/ambiente.

Chaves de idempotência: email reservado dos usuários, nome marcado dos rios, par rio/data UTC das coletas, código da medição por coleta e nome de arquivo reservado da predição. Uma transação engloba a população inteira. Um advisory lock do PostgreSQL serializa execuções concorrentes da rotina. Colisões ou alterações nos dados da fixture interrompem a operação com rollback; a rotina não redefine senhas ou sobrescreve medições existentes. Nomes duplicados preexistentes causam interrupção, não deduplicação destrutiva.

## Dados preparados

Primeira execução em banco sem a fixture: **2 usuários, 6 rios, 9 vínculos usuário/rio, 31 coletas, 248 medições e 30 predições sintéticas**. São 317 registros de entidades e 9 registros de associação. Os totais se referem apenas ao dataset `waterpath-sintetico-rios-v1`, não ao banco inteiro.

Todos os rios/localizações e usuários são identificados por `[SINTETICO v1]`. Os emails são `rios.sinteticos.1@waterpath.invalid` e `rios.sinteticos.2@waterpath.invalid`. O primeiro usuário acompanha todos os rios; o segundo acompanha Aurora, Horizonte e Fronteira. A unidade de `Tamanho` não é especificada no contrato da API: os valores abaixo usam a unidade definida pelo projeto, sem presumir km ou área.

| Rio fictício | Região fictícia | Tamanho | Coletas | Risco mais recente |
| --- | --- | ---: | ---: | --- |
| Aurora | Vale Norte / AM | 3,5 | 6 | 1 — baixo |
| das Pedras Azuis | Serra / MG | 18 | 6 | 3 — alto, pelo histórico |
| Horizonte | Planície / MS | 62,75 | 6 | 3 — alto |
| Recorrência | Bacia / SP | 140 | 6 | 2 — moderado, pelo histórico |
| Fronteira | Campos / RS | 480,25 | 6 | 1 — baixo, pH no limite 9 |
| Sem Análise | Sertão / BA | 0,8 | 1 | 404 — sem resultado válido |

Datas fixas entre 01 e 06/09/2026, sempre às 12:00 UTC. Cada coleta tem temperatura, pH, OD, condutividade, sólidos suspensos, carbono orgânico, fósforo total e nitrato censurado sem valor, com limite positivo. Coordenadas e profundidades são fictícias.

As predições são fixtures determinísticas, não resultados de execução da IA. Usam PNG de um pixel, `synthetic: true`, identificação do dataset e motivos sintéticos. São validadas por `ContratoRisco.Valido` antes de salvar e reproduzem a estrutura atual de histórico, versões e vínculos. Não representam análise ambiental real. O serviço de IA e os modelos não são chamados; inferência multimodal não está coberta por esta coleção.

## Importar e executar no Insomnia

1. Inicie a API local com `dotnet run --launch-profile http`, depois de configurar a conexão correta e `JWT_KEY` localmente.
2. Importe `testing/insomnia-waterpath-rios.json` pela opção de importação de arquivo do Insomnia. O formato v4 reaproveita as coleções antigas de `bin/Debug/net9.0`, atualizando DTOs, autenticação e scripts; as antigas foram preservadas.
3. Crie um **subambiente privado**. Configure `base_url` sem barra final (padrão `http://localhost:5189`), `seed_password` com a senha fornecida à população e `confirm_dev_database: true` somente após confirmar o banco. `seed_email` já aponta para o primeiro usuário sintético. `token` e IDs começam vazios/zero e são capturados automaticamente.
4. No **Collection Runner**, selecione as requisições dos grupos `00` a `05` em ordem numérica. Desative a interrupção no primeiro erro para que a limpeza seja executada após as divergências esperadas. Use uma iteração por vez e não execute fluxos concorrentes no mesmo ambiente.
5. O grupo `06` testa cadastro de usuário e é opcional na execução rotineira. A API não expõe exclusão de usuários: os usuários criados nesse grupo permanecem identificados como sintéticos. O caso de senha ausente também pode criar uma conta devido à divergência atual. Seus IDs são guardados em `created_user_id` e `missing_password_user_id`.

O login captura `token`/`user_id`. A listagem captura os seis IDs da fixture; a consulta de coletas captura `seed_collection_id`; risco captura `seed_prediction_id`. O POST de rio retorna apenas mensagem HTTP 200, por isso o ID é recuperado pela rota autenticada de nome com handler disponível. Os nomes de execução usam timestamp e componente aleatório e são codificados para URL.

Os scripts usam `insomnia.test`, `insomnia.expect`, `insomnia.response`, `insomnia.environment`, `insomnia.request.body.update` e `insomnia.sendRequest`, conforme a [documentação oficial de scripts](https://developer.konghq.com/insomnia/scripts/). A execução recomendada é o [Collection Runner](https://developer.konghq.com/insomnia/test/), que processa os scripts de resposta. Não use os testes unitários legados como substituto desses scripts.

## Limpeza e retomada

Somente IDs capturados do rio criado na execução atual são usados para exclusão. Antes de excluir, os scripts consultam o rio com autenticação e conferem nome, localização, usuário e nonce; coletas também têm seu vínculo conferido. As fixtures da população não são usadas como alvos de exclusão.

Se a execução for interrompida, preserve as variáveis da execução e rode as requisições de limpeza correspondentes aos registros ainda existentes. Se a coleta já foi excluída ou nunca chegou a ser criada, rode apenas a exclusão/verificação do rio. Uma nova execução do cadastro de rio é bloqueada enquanto houver `owned_run`/ID pendente. Não substitua esses IDs por IDs da fixture ou de dados existentes. O último GET do rio confirma 404 e zera os IDs de execução. Não há exclusão automática dos usuários.

## Verificação auxiliar e testes .NET

Na raiz:

```powershell
node back-end/api/testing/generate-insomnia.cjs
node back-end/api/testing/run-collection.cjs --check
dotnet test back-end/api.Tests/WaterPath.Api.Tests.csproj
```

`--check` valida formato, parentes, IDs únicos e sintaxe dos scripts; não faz HTTP. `RiverSeederTests` testa a população duas vezes, preservação de dados existentes, cenários de risco e rollback em colisão. `RiverCollectionHttpTests` inicia os controllers reais em HTTP local e SQLite em memória, fornece senha/chave aleatórias somente nos processos e executa a coleção pelo verificador Node. Esse teste registra divergências conhecidas e exige que todas as demais verificações e a limpeza passem; um teste .NET aprovado não significa que as asserções de sucesso das rotas com divergência passaram.

Para conferir a API local usando um PostgreSQL de desenvolvimento/testes já populado, use o terminal onde a senha foi definida, partindo da raiz:

```powershell
$env:WATERPATH_TEST_BASE_URL = 'http://localhost:5189'
$env:WATERPATH_TEST_CONFIRM_DEV_DATABASE = 'true'
node back-end/api/testing/run-collection.cjs
Remove-Item Env:WATERPATH_SEED_PASSWORD -ErrorAction SilentlyContinue
```

O verificador Node é complementar: interpreta os scripts gerados com uma implementação pequena das APIs usadas, faz HTTP real e informa falhas, mas **não comprova importação ou execução no runtime nativo do Insomnia**. Ele roda todos os grupos, inclusive cadastro de usuários. Não imprime senhas, tokens ou corpos das respostas. `WATERPATH_TEST_REPORT` pode apontar para um arquivo local de relatório sanitizado. As rotas com divergência fazem o verificador retornar código 1; não amplie os status aceitos para esconder as falhas.

Veja os resultados desta execução e os impedimentos em [VERIFICACAO.md](VERIFICACAO.md).
