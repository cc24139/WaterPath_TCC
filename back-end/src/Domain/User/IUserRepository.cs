namespace Domain.User
{
    public interface IUserRepository
    {
        void Cadastrar(User user);
        void Atualizar(User user);
        void Deletar(int id);
        User ObterPorId(int id);
        List<User> ObterTodos();
    }
}
