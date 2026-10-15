using System.Collections.Generic;

namespace back_end.src.Domain.MetalPesado
{
    public interface IMetalPesadoRepository
    {
        Task Cadastrar(MetalPesadoEntity metalPesado, CancellationToken cancellationToken = default);
        Task Atualizar(MetalPesadoEntity metalPesado, CancellationToken cancellationToken = default);
        Task Deletar(int id, CancellationToken cancellationToken = default);
        Task<MetalPesadoEntity?> ObterPorId(int id, CancellationToken cancellationToken = default);
        Task<List<MetalPesadoEntity>> ObterTodos(CancellationToken cancellationToken = default);
    }
}
