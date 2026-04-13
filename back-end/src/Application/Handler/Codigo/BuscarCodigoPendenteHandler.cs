using System.Threading;
using System.Threading.Tasks;
using Application.Queries.Codigo;
using back_end.src.Domain.Codigo;
using MediatR;

namespace Application.Handler.Codigo;

public class BuscarCodigoPendenteHandler : IRequestHandler<QueryCodigoPendente, bool>
{
    private readonly ICodigoRepository codigoRepository;

    public BuscarCodigoPendenteHandler(ICodigoRepository codigoRepository)
    {
        this.codigoRepository = codigoRepository;
    }

    public Task<bool> Handle(QueryCodigoPendente request, CancellationToken cancellationToken)
    {
        var existeCodigoPendente = codigoRepository.VerificarPendenciaCodigo(request.UsuarioId);
        return Task.FromResult(existeCodigoPendente);
    }
}
