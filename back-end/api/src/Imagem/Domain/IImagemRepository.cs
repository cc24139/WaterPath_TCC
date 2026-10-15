using System.Collections.Generic;

namespace back_end.src.Domain.Imagem
{
    public interface IImagemRepository
    {
        Task Cadastrar(ImagemEntity imagem, CancellationToken cancellationToken = default);
        Task Atualizar(ImagemEntity imagem, CancellationToken cancellationToken = default);
        Task Deletar(int id, CancellationToken cancellationToken = default);
        Task<ImagemEntity?> ObterPorId(int id, CancellationToken cancellationToken = default);
        Task<List<ImagemEntity>> ObterTodos(CancellationToken cancellationToken = default);
        Task<List<ImagemEntity>> ObterImagensPorCorpoHidrico(int corpoHidricoId, CancellationToken cancellationToken = default);
        Task<List<ImagemEntity>> ObterImagensPorColeta(int coletaId, CancellationToken cancellationToken = default);
        Task<List<ImagemEntity>> ObterImagensPorPeriodo(int corpoHidricoId, string dataInicio, string dataFim, CancellationToken cancellationToken = default);
    }
}
