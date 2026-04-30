using System.Threading;
using System.Threading.Tasks;
using Application.Commands.Codigo;
using back_end.src.Domain.Codigo;
using Domain.User;
using MediatR;

namespace Application.Handler.Codigo;

public class GerarCodigoHandler : IRequestHandler<CommandGerarCodigo, string>
{
    private readonly ICodigoRepository codigoRepository;
    private readonly IUserRepository userRepository;

    public GerarCodigoHandler(ICodigoRepository codigoRepository, IUserRepository userRepository)
    {
        this.codigoRepository = codigoRepository;
        this.userRepository = userRepository;
    }

    public Task<string> Handle(CommandGerarCodigo request, CancellationToken cancellationToken)
    {
        var usuario = userRepository.ObterUsuarioPorId(request.UsuarioId);
        if (usuario == null)
        {
            throw new ArgumentException("Usuário não encontrado");
        }

        var existeCodigoPendente = codigoRepository.VerificarPendenciaCodigo(request.UsuarioId);
        if (existeCodigoPendente)
        {
            throw new ArgumentException("Já existe um código pendente para este usuário");
        }

        var codigo = new CodigoEntity(usuario);
        codigo.Codigo = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
        var codigoGerado = codigoRepository.GerarCodigo(codigo);

        return Task.FromResult(codigoGerado);
    }
}
