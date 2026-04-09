namespace Core.Interfaces;

using System.Reflection.Metadata;
using Core.User;
public interface IUserRepository
{
    Task<> Login(User user);
    Task<User> ObterUsuarioPorId(int id);
    Task<> CadastrarUsuario(User user);
    Task<> AlterarInformacoes(User user);
    Task<> ExcluirUsuario(User user);
}