using System.Collections.Generic;

namespace back_end.src.Domain.QualidadeFutura
{
    public interface IQualidadeFuturaRepository
    {
        Task Cadastrar(QualidadeFuturaEntity qualidadeFutura, CancellationToken cancellationToken = default);
        Task Atualizar(int id, QualidadeFuturaEntity qualidadeFutura, CancellationToken cancellationToken = default);
        Task Deletar(int id, CancellationToken cancellationToken = default);
        Task<QualidadeFuturaEntity?> ObterPorId(int id, CancellationToken cancellationToken = default);
        Task<List<QualidadeFuturaEntity>> ObterTodos(CancellationToken cancellationToken = default);
    }
}
