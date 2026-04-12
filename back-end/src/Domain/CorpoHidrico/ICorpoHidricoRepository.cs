using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back_end.src.Domain.CorpoHidrico
{
    public interface ICorpoHidricoRepository
    {
        //Crud
        public void Cadastrar(CorpoHidricoEntity corpoHidrico);
        public void Atualizar(CorpoHidricoEntity corpoHidrico);
        public void Deletar(int id);
        public CorpoHidricoEntity ObterCorpoHidricoPorId(int id);
        //Relacionamento
        public List<CorpoHidricoEntity> ObterTodosCorposHidricos(int idUser);
        public void CadastrarUsuarioCorpoHidrico(int idUser, int idCorpoHidrico);
        public void DeletarUsuarioCorpoHidrico(int idUser, int idCorpoHidrico);

    }
}
