using Application.Queries.CorpoHidrico;
using back_end.src.Domain.CorpoHidrico;
using MediatR;

namespace Application.Handler.CorpoHidrico;

public class ObterCorposHidricosPorUsuarioHandler
    : IRequestHandler<QueryObterCorposHidricosPorUsuario, List<CorpoHidricoEntity>>
{
    private readonly ICorpoHidricoRepository corpoHidricoRepository;

    public ObterCorposHidricosPorUsuarioHandler(ICorpoHidricoRepository corpoHidricoRepository)
    {
        this.corpoHidricoRepository = corpoHidricoRepository;
    }

    public Task<List<CorpoHidricoEntity>> Handle(
        QueryObterCorposHidricosPorUsuario request,
        CancellationToken cancellationToken
    )
    {
        var corposHidricos = corpoHidricoRepository.ObterCorposHidricosPorUsuario(request.UserId);
        return Task.FromResult(corposHidricos);
    }
}
