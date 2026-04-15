using Application.Queries.CorpoHidrico;
using back_end.src.Domain.CorpoHidrico;
using MediatR;

namespace Application.Handler.CorpoHidrico;

public class ObterUsuariosDoCorpoHidricoHandler
    : IRequestHandler<QueryObterUsuarioDOCorpoHidrico, List<int>>
{
    private readonly ICorpoHidricoRepository corpoHidricoRepository;

    public ObterUsuariosDoCorpoHidricoHandler(ICorpoHidricoRepository corpoHidricoRepository)
    {
        this.corpoHidricoRepository = corpoHidricoRepository;
    }

    public Task<List<int>> Handle(
        QueryObterUsuarioDOCorpoHidrico request,
        CancellationToken cancellationToken
    )
    {
        var usuariosIds = corpoHidricoRepository.ObterUsuariosDoCorpoHidrico(
            request.CorpoHidricoId
        );
        return Task.FromResult(usuariosIds);
    }
}
