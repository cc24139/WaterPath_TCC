using Application.Queries.CorpoHidrico;
using back_end.src.Domain.CorpoHidrico;
using MediatR;

namespace Application.Handler.CorpoHidrico;

public class ObterColetasDoCorpoHidricoHandler
    : IRequestHandler<QueryObterColetasDoCorpoHidrico, List<int>>
{
    private readonly ICorpoHidricoRepository corpoHidricoRepository;

    public ObterColetasDoCorpoHidricoHandler(ICorpoHidricoRepository corpoHidricoRepository)
    {
        this.corpoHidricoRepository = corpoHidricoRepository;
    }

    public Task<List<int>> Handle(
        QueryObterColetasDoCorpoHidrico request,
        CancellationToken cancellationToken
    )
    {
        return corpoHidricoRepository.ObterColetasDoCorpoHidrico(request.CorpoHidricoId, cancellationToken);
    }
}
