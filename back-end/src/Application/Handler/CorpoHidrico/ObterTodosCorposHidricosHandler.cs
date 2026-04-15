using Application.Queries.CorpoHidrico;
using back_end.src.Domain.CorpoHidrico;
using MediatR;

namespace Application.Handler.CorpoHidrico;

public class ObterTodosCorposHidricosHandler
    : IRequestHandler<QueryObterTodosCorposHidricos, List<CorpoHidricoEntity>>
{
    private readonly ICorpoHidricoRepository corpoHidricoRepository;

    public ObterTodosCorposHidricosHandler(ICorpoHidricoRepository corpoHidricoRepository)
    {
        this.corpoHidricoRepository = corpoHidricoRepository;
    }

    public Task<List<CorpoHidricoEntity>> Handle(
        QueryObterTodosCorposHidricos request,
        CancellationToken cancellationToken
    )
    {
        var corposHidricos = corpoHidricoRepository.ObterTodos();
        return Task.FromResult(corposHidricos);
    }
}
