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
        CorpoHidricoEntity? ObterCorpoHidricoPorId(int id);
        List<CorpoHidricoEntity> ObterTodos();
        List<CorpoHidricoEntity> ObterCorposHidricosPorUsuario(int userId);
        List<int> ObterUsuariosDoCorpoHidrico(int corpoHidricoId);
        List<int> ObterColetasDoCorpoHidrico(int corpoHidricoId);

        void AdicionarUsuario(int corpoHidricoId, int userId);
        void DeletarUsuario(int corpoHidricoId, int userId);
    }
}
