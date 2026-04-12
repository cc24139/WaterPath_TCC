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
    }
}
