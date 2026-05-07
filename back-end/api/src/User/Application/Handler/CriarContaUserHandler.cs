namespace Application.Handler.User;

using System.Threading;
using System.Threading.Tasks;
using Application.Commands.User;
using Domain.User;
using MediatR;

public class CriarContaUserHandler : IRequestHandler<CommandCriarConta, CriarContaResponse>
{
    private readonly IUserRepository userRepository;

    public CriarContaUserHandler(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public Task<CriarContaResponse> Handle(
        CommandCriarConta request,
        CancellationToken cancellationToken
    )
    {
        var existingUser = userRepository.ObterUsuarioPorEmail(request.Email);
        if (existingUser != null)
        {
            throw new System.ArgumentException("Email já cadastrado");
        }

        var user = new UserEntity(request.Nome, request.Senha, request.Email);
        userRepository.Cadastrar(user);

        return Task.FromResult(
            new CriarContaResponse { UsuarioId = user.Id, Mensagem = "Conta criada com sucesso" }
        );
    }
}
