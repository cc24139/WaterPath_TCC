namespace Domain.User
{
    public interface IUserRepository
    {
        //Crud
        void Cadastrar(User user);
        void AlterarUsuario(User user);
        void Deletar(int id);

        User ObterUsuarioPorEmail(string email);
        User ObterUsuarioPorId(int id);

        //Ações

        void EsquecerSenha(string email);

        bool Login(string email, string senha);

        bool VerificarCodigo(string email, string codigo);
    }
}
