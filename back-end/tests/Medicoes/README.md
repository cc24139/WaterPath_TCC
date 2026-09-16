# Verificações de coleta e medições

Execute `dotnet run --project tests/Medicoes/Medicoes.Checks.csproj` a partir de `back-end`.

Valida domínio, handlers via MediatR, respostas do controller de medições, entrada JSON de coleta e o modelo real de produção do Entity Framework/PostgreSQL. Verifica as FKs obrigatórias CorpoHidrico → Coletas → Medicoes, o índice único por coleta/tipo e a vinculação de novas medições pelo EF. Não executa CRUD contra um PostgreSQL real.

O `Program.cs` já registra os repositórios e configura a serialização para ignorar ciclos.

No POST/PUT de coleta, `medicoes` aceita objetos no formato `InputMedicao` (`codigoMedicao`, `valor`, `unidade`, `ehCensurado`, `limite`). O PUT adiciona ou atualiza medições por código e preserva as omitidas. Para excluir, use o endpoint de medições.

Para bancos no esquema das migrations antigas, `api/Infrastructure/Data/Scripts/AtualizarColetas.sql` adapta as colunas de coleta sem apagar os dados antigos; `CreateMedicoes.sql` cria a tabela de medições se ainda não existir. Esses scripts não foram executados no banco e são independentes do histórico EF. O snapshot antigo ainda precisa ser reconciliado antes de retomar o fluxo de migrations (incluindo a remoção prévia de Codigo no projeto).
