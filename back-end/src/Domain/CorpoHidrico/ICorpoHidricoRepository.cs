using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back_end.src.Domain.CorpoHidrico
{
    public interface ICorpoHidricoRepository
    {
        void Cadastrar(CorpoHidricoEntity corpoHidrico);
        void Atualizar(CorpoHidricoEntity corpoHidrico);
        void Deletar(int id);
        CorpoHidricoEntity ObterPorId(int id);
        List<CorpoHidricoEntity> ObterTodos();
    }
}
