namespace Domain.User
{

    public interface IUserRepository
    {
        void Cadastrar(User user);
        User ObterUsuarioPorEmail(string email);
        User ObterUsuarioPorId(int id);

        void EsquecerSenha(string email);
        void AlterarUsuario(User user);

        bool Login(string email, string senha);

    }
}