using System.Collections.Generic;

namespace back_end.src.Domain.Coleta
{
    public interface IColetaRepository
    {
        Task Cadastrar(ColetaEntity coleta, int idCorpoHidrico, CancellationToken cancellationToken = default);
        Task Atualizar(ColetaEntity coleta, int idColeta, CancellationToken cancellationToken = default);
        Task Deletar(int id, CancellationToken cancellationToken = default);
        Task<ColetaEntity?> ObterPorId(int id, CancellationToken cancellationToken = default);
        Task<List<ColetaEntity>> ObterTodos(CancellationToken cancellationToken = default);
        Task CadastrarListaColetas(List<ColetaEntity> coletas, CancellationToken cancellationToken = default);
        Task<List<ColetaEntity>> ObterPorPeriodo(int corpoHidricoId, string dataInicio, string dataFim, CancellationToken cancellationToken = default);
        Task<List<ColetaEntity>> ObterPorCorpoHidrico(int corpoHidricoId, CancellationToken cancellationToken = default);
        Task<List<ColetaEntity>> ObterPorCorpoHidricoEPeriodo(int corpoHidricoId, string dataInicio, string dataFim, CancellationToken cancellationToken = default);
        Task<List<ColetaEntity>> ObterPorData(DateTime data, CancellationToken cancellationToken = default);
    }
}
