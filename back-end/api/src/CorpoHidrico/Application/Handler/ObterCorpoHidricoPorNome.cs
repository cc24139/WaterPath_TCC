using Application.Queries.CorpoHidrico;
using back_end.src.Domain.CorpoHidrico;
using MediatR;

namespace Application.Handler.CorpoHidrico;

public class ObterCorpoHidricoPorNome
    : IRequestHandler<CorpoHidricoNomeCommand, CorpoHidricoEntity?>
{
    private readonly ICorpoHidricoRepository repository;

    public ObterCorpoHidricoPorNome(ICorpoHidricoRepository repository)
    {
        this.repository = repository;
    }

    public Task<CorpoHidricoEntity?> Handle(
        CorpoHidricoNomeCommand request,
        CancellationToken cancellationToken
    )
    {
        var corpoHidrico = repository.ObterCorpoHidricoPorNome(request.Nome);
        return Task.FromResult(corpoHidrico);
    }
}
