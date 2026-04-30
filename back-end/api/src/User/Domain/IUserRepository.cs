using System.Collections.Generic;

namespace Domain.User
{
    public interface IUserRepository
    {
        void Cadastrar(UserEntity user);
        void Atualizar(UserEntity user);
        void Deletar(int id);
        UserEntity ObterUsuarioPorId(int id);
        UserEntity ObterUsuarioPorEmail(string email);
        List<UserEntity> ObterTodos();
    }
}
