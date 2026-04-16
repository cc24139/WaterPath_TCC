using Application.Commands.QualidadeFutura;
using Application.Queries.QualidadeFutura;
using back_end.src.Domain.QualidadeFutura;
using MediatR;

namespace Application.Handler.QualidadeFutura;

public class CadastrarQualidadeFuturaHandler
    : IRequestHandler<CommandCadastrarQualidadeFutura, Unit>
{
    private readonly IQualidadeFuturaRepository qualidadeFuturaRepository;

    public CadastrarQualidadeFuturaHandler(IQualidadeFuturaRepository qualidadeFuturaRepository)
    {
        this.qualidadeFuturaRepository = qualidadeFuturaRepository;
    }

    public Task<Unit> Handle(
        CommandCadastrarQualidadeFutura request,
        CancellationToken cancellationToken
    )
    {
        qualidadeFuturaRepository.Cadastrar(request.QualidadeFutura);
        return Task.FromResult(Unit.Value);
    }
}

public class AtualizarQualidadeFuturaHandler
    : IRequestHandler<CommandAtualizarQualidadeFutura, Unit>
{
    private readonly IQualidadeFuturaRepository qualidadeFuturaRepository;

    public AtualizarQualidadeFuturaHandler(IQualidadeFuturaRepository qualidadeFuturaRepository)
    {
        this.qualidadeFuturaRepository = qualidadeFuturaRepository;
    }

    public Task<Unit> Handle(
        CommandAtualizarQualidadeFutura request,
        CancellationToken cancellationToken
    )
    {
        qualidadeFuturaRepository.Atualizar(request.QualidadeFutura);
        return Task.FromResult(Unit.Value);
    }
}

public class DeletarQualidadeFuturaHandler : IRequestHandler<CommandDeletarQualidadeFutura, Unit>
{
    private readonly IQualidadeFuturaRepository qualidadeFuturaRepository;

    public DeletarQualidadeFuturaHandler(IQualidadeFuturaRepository qualidadeFuturaRepository)
    {
        this.qualidadeFuturaRepository = qualidadeFuturaRepository;
    }

    public Task<Unit> Handle(
        CommandDeletarQualidadeFutura request,
        CancellationToken cancellationToken
    )
    {
        qualidadeFuturaRepository.Deletar(request.Id);
        return Task.FromResult(Unit.Value);
    }
}

public class ObterQualidadeFuturaPorIdHandler
    : IRequestHandler<QueryObterQualidadeFuturaPorId, QualidadeFuturaEntity?>
{
    private readonly IQualidadeFuturaRepository qualidadeFuturaRepository;

    public ObterQualidadeFuturaPorIdHandler(IQualidadeFuturaRepository qualidadeFuturaRepository)
    {
        this.qualidadeFuturaRepository = qualidadeFuturaRepository;
    }

    public Task<QualidadeFuturaEntity?> Handle(
        QueryObterQualidadeFuturaPorId request,
        CancellationToken cancellationToken
    )
    {
        var qualidadeFutura = qualidadeFuturaRepository.ObterPorId(request.Id);
        return Task.FromResult(qualidadeFutura);
    }
}

public class ObterTodasQualidadesFuturasHandler
    : IRequestHandler<QueryObterTodasQualidadesFuturas, List<QualidadeFuturaEntity>>
{
    private readonly IQualidadeFuturaRepository qualidadeFuturaRepository;

    public ObterTodasQualidadesFuturasHandler(IQualidadeFuturaRepository qualidadeFuturaRepository)
    {
        this.qualidadeFuturaRepository = qualidadeFuturaRepository;
    }

    public Task<List<QualidadeFuturaEntity>> Handle(
        QueryObterTodasQualidadesFuturas request,
        CancellationToken cancellationToken
    )
    {
        var qualidadesFuturas = qualidadeFuturaRepository.ObterTodos();
        return Task.FromResult(qualidadesFuturas);
    }
}
