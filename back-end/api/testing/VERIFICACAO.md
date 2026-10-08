# Verificação em 07/10/2026

## Resultado e limite da execução

Implementação e coleção concluídas. **Nenhum registro foi inserido no PostgreSQL configurado.** Os dois `.env` apontam para o mesmo Neon remoto, banco `neondb`, sem indicação verificável de desenvolvimento/testes. Foi solicitada confirmação dessa finalidade, ainda sem resposta nesta execução. O comando de população não recebeu `WATERPATH_SEED_TARGET` confirmado e recusou a gravação antes de acessar o banco. Os dados existentes foram preservados.

Uma consulta somente de leitura ao histórico EF do Neon foi executada. Encontrou estas migrations pendentes:

- `20261004181139_PersistirPredicoesIA`
- `20261007120000_ReferenciarDadosClassificacao`

A rotina também recusa população enquanto houver migrations pendentes. Nenhuma migration foi aplicada. Para concluir a etapa no PostgreSQL, é necessário identificar/confirmar o banco de desenvolvimento/testes, atualizar seu esquema após revisão e fornecer a senha de teste pelo ambiente. Não há credenciais nos arquivos entregues.

## Executado efetivamente

| Verificação | Resultado |
| --- | --- |
| Build final normal da API, .NET 10.0.401 | Sucesso, 0 erros e 62 avisos já presentes no código existente |
| Suíte .NET completa | 73 casos: 72 aprovados, 1 ignorado, 0 falhas |
| População em SQLite temporário, primeira execução | 2 usuários, 6 rios, 9 vínculos, 31 coletas, 248 medições e 30 predições |
| População em SQLite, segunda execução | Zero registros/vínculos inseridos; mesmos IDs |
| Preservação de rio preexistente | Confirmada no teste de idempotência |
| Colisão no terceiro rio | Rollback de usuários, rios, coletas, medições e predições inseridos na transação |
| Cenários de risco | Últimos resultados 1, 3, 3, 2 e 1; sexto rio sem risco válido |
| Estrutura/sintaxe da coleção | 65 requisições, 130 scripts válidos, IDs únicos e parentes existentes |
| Coleção pelo verificador Node contra controllers reais em HTTP local/SQLite | 65 requisições principais; 151 asserções, 145 aprovadas e 6 falhas de contrato abaixo |
| Limpeza após as falhas | Restaram exatamente as 6 fixtures, 31 coletas, 248 medições e 30 predições; registros descartáveis do fluxo removidos |
| PostgreSQL configurado | Somente consulta de migrations; 0 inserções e 0 alterações de esquema |

O total da primeira população de teste é **317 registros de entidades mais 9 vínculos**. Os testes usam bancos temporários independentes: esses números descrevem uma execução bem-sucedida do dataset, não a soma de todas as execuções da suíte. O fluxo HTTP também cria usuários sintéticos pelo cadastro, incluindo o caso sem senha aceito indevidamente; eles existem somente no SQLite temporário nesta verificação.

`RiverCollectionHttpTests` passa porque exige exatamente as divergências conhecidas e nenhuma outra falha, além de exigir a limpeza e preservação das fixtures. O verificador Node retorna **código 1**, pois as seis asserções de contrato continuam falhando. Não foram alterados os status esperados para transformar essas divergências em sucesso.

## Divergências confirmadas por HTTP

| Requisição | Esperado | Observado | Causa no código atual |
| --- | ---: | ---: | --- |
| `GET /api/corpohidrico/nome/{nome}` de fixture existente | 200 | 500 | Envia `QueryObterCorpoHidricoPorNome`, mas só existe handler para `CorpoHidricoNomeCommand` |
| Mesma rota com nome inexistente | 404 | 500 | Mesmo handler ausente; não chega à consulta |
| `GET /api/coleta/periodo/{id}` autenticado | 200 | 409 | `QueryObterColetasPorPeriodo` não tem handler; controller transforma `InvalidOperationException` em conflito |
| `PUT /api/corpohidrico/{id}` de registro existente, dados válidos | 200 | 400 | Handler constrói nova entidade sem atribuir o ID; repository procura ID zero |
| `GET /api/medicoes` | 200 | 500 | O método de exclusão também tem `[HttpGet]` sem template, concorrendo com a listagem |
| `POST /api/user/cadastro` sem `senha` | 400 | 200 | Command preenche senha ausente com string vazia; cadastro a transforma em hash e aceita a conta |

Esses problemas foram registrados, sem mudar os contratos/handlers existentes nesta tarefa. A captura de ID por nome usa a rota autenticada `GET /api/corpohidrico/usuario/{nome}`, que funciona.

Outras observações do contrato atual:

- `DELETE /api/corpohidrico/{id}` é público; a exclusão sem token de um rio criado pelo próprio teste retornou 200. Listagem e consulta pública por nome também não têm `[Authorize]`. Não há teste que presuma 401 para essas rotas públicas.
- As nove requisições geradas sem token para rotas realmente protegidas retornaram 401, com resposta vazia.
- `GET /api/corpohidrico/usuario/{nome}` consulta por nome globalmente, sem filtrar o vínculo do usuário. Isso foi identificado pela leitura do controller/handler; não foi executado um teste de acesso entre usuários nesta tarefa.
- Cadastro de rio retorna mensagem HTTP 200, sem ID/Location. Consulta por usuário usa o usuário do token; não recebe um ID de usuário pela URL.
- Inexistência em PUT de rio retorna 400 pelo contrato atual; IDs não positivos de risco/medição retornam 400; risco inexistente com ID positivo retorna 404; rio existente sem resultado válido retorna 404. A coleção distingue esses casos.
- Não há endpoint de exclusão de usuário. O grupo de cadastro deixa usuários sintéticos no banco quando executado contra PostgreSQL.

## Gerado, mas não executado nativamente

A coleção contém os fluxos de autenticação, consultas, cadastro, atualização, validação, ausência de autenticação e exclusão, com captura de tokens/IDs e asserções oficiais de scripts do Insomnia. Foi conferida pelo verificador Node e executada contra HTTP real. **Importação no aplicativo Insomnia e execução no runtime nativo Insomnia/Inso não foram realizadas**; não havia `inso` disponível neste ambiente. O verificador complementar não substitui essa confirmação.

O teste existente de inferência real foi ignorado por ausência de `WATERPATH_IA_TEST_URL` e `WATERPATH_TEST_IMAGE`. As predições de população são sintéticas; não houve inferência ou treinamento de modelos.

As primeiras tentativas de `dotnet test` na sandbox falharam na comunicação com o testhost. A execução fora da sandbox foi permitida pelo ambiente e passou. Houve também um bloqueio transitório do arquivo de cache de assets pelo Windows; um teste intermediário desabilitou essa etapa. O **build e a suíte finais passaram com a configuração normal**, sem essa exceção.

## Arquivos desta tarefa

- `.gitignore`: protege `.env` da API e relatórios/ambientes locais.
- `README.md`: link para as instruções e o relatório.
- `back-end/api/Program.cs`: comandos explícitos de consulta/população; variáveis do processo prevalecem sobre `.env`.
- `back-end/api/Infrastructure/Data/Seeding/RiverSeedCommand.cs`: guardas de ambiente/alvo/senha, migrations e manifesto sem segredos.
- `back-end/api/Infrastructure/Data/Seeding/RiverSeeder.cs`: dataset e transação idempotente.
- `back-end/api/testing/generate-insomnia.cjs`: gerador reproduzível da coleção v4.
- `back-end/api/testing/insomnia-waterpath-rios.json`: coleção importável.
- `back-end/api/testing/run-collection.cjs`: verificador de estrutura e executor HTTP complementar.
- `back-end/api/testing/README.md` e `VERIFICACAO.md`: execução, configuração, importação, limpeza, resultados e impedimentos.
- `back-end/api.Tests/RiverSeederTests.cs`: idempotência, preservação, risco e rollback.
- `back-end/api.Tests/RiverCollectionHttpTests.cs`: execução HTTP dos scripts com contextos EF por requisição.

As alterações de IA já presentes no workspace foram preservadas. As coleções antigas e os scripts legados de esquema também foram preservados.
