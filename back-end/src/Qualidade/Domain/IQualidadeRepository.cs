using System.Collections.Generic;

namespace back_end.src.Domain.Qualidade
{
    public interface IQualidadeRepository
    {
        void Cadastrar(QualidadeEntity qualidade);
        void Atualizar(QualidadeEntity qualidade);
        void Deletar(int id);
        QualidadeEntity ObterPorId(int id);
        List<QualidadeEntity> ObterTodos();
    }
}
