using System.Collections.Generic;

namespace back_end.src.Domain.CianoBacteria
{
    public interface ICianoBacteriaRepository
    {
        void Cadastrar(CianoBacteriaEntity cianoBacteria);
        void Atualizar(CianoBacteriaEntity cianoBacteria);
        void Deletar(int id);
        CianoBacteriaEntity ObterPorId(int id);
        List<CianoBacteriaEntity> ObterTodos();
    }
}
