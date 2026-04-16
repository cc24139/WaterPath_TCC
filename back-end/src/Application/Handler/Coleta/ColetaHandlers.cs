using Application.Commands.Coleta;
using Application.Queries.Coleta;
using back_end.src.Domain.Coleta;
using MediatR;

namespace Application.Handler.Coleta;

public class CadastrarColetaHandler : IRequestHandler<CommandCadastrarColeta, Unit>
{
    private readonly IColetaRepository coletaRepository;

    public CadastrarColetaHandler(IColetaRepository coletaRepository)
    {
        this.coletaRepository = coletaRepository;
    }

    public Task<Unit> Handle(CommandCadastrarColeta request, CancellationToken cancellationToken)
    {
        coletaRepository.Cadastrar(request.Coleta, request.CorpoHidricoId);
        return Task.FromResult(Unit.Value);
    }
}

public class AtualizarColetaHandler : IRequestHandler<CommandAtualizarColeta, Unit>
{
    private readonly IColetaRepository coletaRepository;

    public AtualizarColetaHandler(IColetaRepository coletaRepository)
    {
        this.coletaRepository = coletaRepository;
    }

    public Task<Unit> Handle(CommandAtualizarColeta request, CancellationToken cancellationToken)
    {
        coletaRepository.Atualizar(request.Coleta, request.ColetaId);
        return Task.FromResult(Unit.Value);
    }
}

public class DeletarColetaHandler : IRequestHandler<CommandDeletarColeta, Unit>
{
    private readonly IColetaRepository coletaRepository;

    public DeletarColetaHandler(IColetaRepository coletaRepository)
    {
        this.coletaRepository = coletaRepository;
    }

    public Task<Unit> Handle(CommandDeletarColeta request, CancellationToken cancellationToken)
    {
        coletaRepository.Deletar(request.Id);
        return Task.FromResult(Unit.Value);
    }
}

public class ObterColetaPorIdHandler : IRequestHandler<QueryObterColetaPorId, ColetaEntity?>
{
    private readonly IColetaRepository coletaRepository;

    public ObterColetaPorIdHandler(IColetaRepository coletaRepository)
    {
        this.coletaRepository = coletaRepository;
    }

    public Task<ColetaEntity?> Handle(
        QueryObterColetaPorId request,
        CancellationToken cancellationToken
    )
    {
        var coleta = coletaRepository.ObterPorId(request.Id);
        return Task.FromResult(coleta);
    }
}

public class ObterTodasColetasHandler : IRequestHandler<QueryObterTodasColetas, List<ColetaEntity>>
{
    private readonly IColetaRepository coletaRepository;

    public ObterTodasColetasHandler(IColetaRepository coletaRepository)
    {
        this.coletaRepository = coletaRepository;
    }

    public Task<List<ColetaEntity>> Handle(
        QueryObterTodasColetas request,
        CancellationToken cancellationToken
    )
    {
        var coletas = coletaRepository.ObterTodos();
        return Task.FromResult(coletas);
    }
}
