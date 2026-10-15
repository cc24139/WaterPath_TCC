using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back_end.src.Domain.CorpoHidrico
{
    public interface ICorpoHidricoRepository
    {
        Task Cadastrar(CorpoHidricoEntity corpoHidrico, CancellationToken cancellationToken = default);
        Task Atualizar(CorpoHidricoEntity corpoHidrico, CancellationToken cancellationToken = default);
        Task Deletar(int id, CancellationToken cancellationToken = default);
        Task<CorpoHidricoEntity?> ObterCorpoHidricoPorId(int id, CancellationToken cancellationToken = default);
        Task<List<CorpoHidricoEntity>> ObterTodos(CancellationToken cancellationToken = default);
    
        Task<CorpoHidricoEntity?> ObterCorpoHidricoPorNome(string nome, CancellationToken cancellationToken = default);
        Task<List<CorpoHidricoEntity>> ObterCorposHidricosPorUsuario(int userId, CancellationToken cancellationToken = default);
        Task<List<int>> ObterUsuariosDoCorpoHidrico(int corpoHidricoId, CancellationToken cancellationToken = default);
        Task<List<int>> ObterColetasDoCorpoHidrico(int corpoHidricoId, CancellationToken cancellationToken = default);
        IAsyncEnumerable<ResultadoRiscoPersistido> ObterResultadosRisco(int corpoHidricoId);

        Task AdicionarUsuario(int corpoHidricoId, int userId, CancellationToken cancellationToken = default);
        Task DeletarUsuario(int corpoHidricoId, int userId, CancellationToken cancellationToken = default);
    }
}
