using System.Threading;
using System.Threading.Tasks;
using Application.Commands.Codigo;
using back_end.src.Domain.Codigo;
using MediatR;

namespace Application.Handler.Codigo;

public class ValidarCodigoHandler : IRequestHandler<CommandValidarCodigo, string>
{
    private readonly ICodigoRepository codigoRepository;

    public ValidarCodigoHandler(ICodigoRepository codigoRepository)
    {
        this.codigoRepository = codigoRepository;
    }

    public Task<string> Handle(CommandValidarCodigo request, CancellationToken cancellationToken)
    {
        var sucesso = codigoRepository.MarcarCodigoComoUsado(request.UsuarioId, request.Codigo);
        if (!sucesso)
        {
            throw new ArgumentException("Código inválido, expirado ou já utilizado");
        }

        return Task.FromResult("Código validado com sucesso");
    }
}
