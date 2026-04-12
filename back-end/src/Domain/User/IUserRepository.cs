using System.Collections.Generic;

namespace Domain.User
{
    public interface IUserRepository
    {
        void Cadastrar(User user);
        void Atualizar(User user);
        void Deletar(int id);
        User ObterUsuarioPorId(int id);
        User ObterUsuarioPorEmail(string email);
        List<User> ObterTodos();
    }
}
