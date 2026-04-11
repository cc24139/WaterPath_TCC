namespace Application.Handler.User;

using System.Threading.Tasks;
using Application.Commands.User;
using Domain.User;


public class CriarContaUserHandler
{
    private readonly IUserRepository userRepository;

    public CriarContaUserHandler(IUserRepository userRepository)
    {
       this.userRepository = userRepository;
    }

    public  int Handle(CommandCriarConta command)
    {
        var usarioExiste =  userRepository.ObterUsuarioPorEmail(command.Email);
        if (usarioExiste != null)
        {
            throw new ArgumentException("Email já cadastrado");
        }
        var criarConta =  new User(
        );
         userRepository.Cadastrar(criarConta);
        return criarConta.Id;
    }
}