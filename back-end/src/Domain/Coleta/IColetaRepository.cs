using System.Collections.Generic;

namespace back_end.src.Domain.Coleta
{
    public interface IColetaRepository
    {
        void Cadastrar(ColetaEntity coleta);
        void Atualizar(ColetaEntity coleta);
        void Deletar(int id);
        ColetaEntity ObterPorId(int id);
        List<ColetaEntity> ObterTodos();
    }
}
