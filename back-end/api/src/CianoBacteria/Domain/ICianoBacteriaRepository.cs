using System.Collections.Generic;

namespace back_end.src.Domain.CianoBacteria
{
    public interface ICianoBacteriaRepository
    {
        Task Cadastrar(CianoBacteriaEntity cianoBacteria, CancellationToken cancellationToken = default);
        Task Atualizar(CianoBacteriaEntity cianoBacteria, CancellationToken cancellationToken = default);
        Task Deletar(int id, CancellationToken cancellationToken = default);
        Task<CianoBacteriaEntity?> ObterPorId(int id, CancellationToken cancellationToken = default);
        Task<List<CianoBacteriaEntity>> ObterTodos(CancellationToken cancellationToken = default);
    }
}
