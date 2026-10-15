using System.Collections.Generic;

namespace back_end.src.Domain.Qualidade
{
    public interface IQualidadeRepository
    {
        Task Cadastrar(QualidadeEntity qualidade, CancellationToken cancellationToken = default);
        Task Atualizar(QualidadeEntity qualidade, CancellationToken cancellationToken = default);
        Task Deletar(int id, CancellationToken cancellationToken = default);
        Task<QualidadeEntity?> ObterPorId(int id, CancellationToken cancellationToken = default);
        Task<List<QualidadeEntity>> ObterTodos(CancellationToken cancellationToken = default);
    }
}
