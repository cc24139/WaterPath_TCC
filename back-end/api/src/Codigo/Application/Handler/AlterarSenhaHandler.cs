
using System.Threading;
using System.Threading.Tasks;
using Application.Commands.Codigo;
using back_end.src.Domain.Codigo;
using MediatR;

namespace Application.Handler.Codigo;

public class AlterarSenhaHandler : IRequestHandler<CommandAlterarSenhaCodigo, string>
{
    private readonly ICodigoRepository codigoRepository;

    public AlterarSenhaHandler(ICodigoRepository codigoRepository)
    {
        this.codigoRepository = codigoRepository;
    }

    public Task<string> Handle(CommandAlterarSenhaCodigo request, CancellationToken cancellationToken)
    {
        var sucesso = codigoRepository.MarcarCodigoComoUsado(request.email, request.codigo);
        if (!sucesso)
        {
            throw new ArgumentException("Código inválido, expirado ou já utilizado");
        }
        codigoRepository.AlterarSenha(request.email, request.senha);
        return Task.FromResult("Código validado com sucesso");
    }
}
