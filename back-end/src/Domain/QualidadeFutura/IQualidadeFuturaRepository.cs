using System.Collections.Generic;

namespace back_end.src.Domain.QualidadeFutura
{
    public interface IQualidadeFuturaRepository
    {
        void Cadastrar(QualidadeFuturaEntity qualidadeFutura);
        void Atualizar(QualidadeFuturaEntity qualidadeFutura);
        void Deletar(int id);
        QualidadeFuturaEntity ObterPorId(int id);
        List<QualidadeFuturaEntity> ObterTodos();
    }
}
