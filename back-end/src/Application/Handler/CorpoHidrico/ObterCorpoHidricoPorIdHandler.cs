using Application.Queries.CorpoHidrico;
using back_end.src.Domain.CorpoHidrico;
using MediatR;

namespace Application.Handler.CorpoHidrico;

public class ObterCorpoHidricoPorIdHandler
    : IRequestHandler<QueryObterCorpoHidricoPorId, CorpoHidricoEntity?>
{
    private readonly ICorpoHidricoRepository corpoHidricoRepository;

    public ObterCorpoHidricoPorIdHandler(ICorpoHidricoRepository corpoHidricoRepository)
    {
        this.corpoHidricoRepository = corpoHidricoRepository;
    }

    public Task<CorpoHidricoEntity?> Handle(
        QueryObterCorpoHidricoPorId request,
        CancellationToken cancellationToken
    )
    {
        var corpoHidrico = corpoHidricoRepository.ObterCorpoHidricoPorId(request.Id);
        return Task.FromResult(corpoHidrico);
    }
}
