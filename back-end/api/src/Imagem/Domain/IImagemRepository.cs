using System.Collections.Generic;

namespace back_end.src.Domain.Imagem
{
    public interface IImagemRepository
    {
        void Cadastrar(ImagemEntity imagem);
        void Atualizar(ImagemEntity imagem);
        void Deletar(int id);
        ImagemEntity ObterPorId(int id);
        List<ImagemEntity> ObterTodos();
        List<ImagemEntity> ObterImagensPorCorpoHidrico(int corpoHidricoId);
        List<ImagemEntity> ObterImagensPorColeta(int coletaId);
        List<ImagemEntity>ObterImagensPorPeriodo(int corpoHidricoId, string dataInicio, string dataFim);
    }
}
