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

    public async Task<CriarContaResponse> Handle(
        CommandCriarConta request,
        CancellationToken cancellationToken
    )
    {
        var existingUser = await userRepository.ObterUsuarioPorEmail(request.Email);
        if (existingUser != null)
        {
            throw new System.ArgumentException("Email já cadastrado");
        }

        var user = new UserEntity(request.Nome, request.Senha, request.Email);
        userRepository.Cadastrar(user);

        return await Task.FromResult(
            new CriarContaResponse
            {
                UsuarioId = user.Id,
                Email = user.Email,
                Mensagem = "Conta criada com sucesso",
            }
        );
    }
}
