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

    public async Task<Unit> Handle(CommandCadastrarColeta request, CancellationToken cancellationToken)
    {
        await coletaRepository.Cadastrar(request.Coleta, request.CorpoHidricoId, cancellationToken);
        return Unit.Value;
    }
}

public class AtualizarColetaHandler : IRequestHandler<CommandAtualizarColeta, Unit>
{
    private readonly IColetaRepository coletaRepository;

    public AtualizarColetaHandler(IColetaRepository coletaRepository)
    {
        this.coletaRepository = coletaRepository;
    }

    public async Task<Unit> Handle(CommandAtualizarColeta request, CancellationToken cancellationToken)
    {
        await coletaRepository.Atualizar(request.Coleta, request.ColetaId, cancellationToken);
        return Unit.Value;
    }
}

public class DeletarColetaHandler : IRequestHandler<CommandDeletarColeta, Unit>
{
    private readonly IColetaRepository coletaRepository;

    public DeletarColetaHandler(IColetaRepository coletaRepository)
    {
        this.coletaRepository = coletaRepository;
    }

    public async Task<Unit> Handle(CommandDeletarColeta request, CancellationToken cancellationToken)
    {
        await coletaRepository.Deletar(request.Id, cancellationToken);
        return Unit.Value;
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
        return coletaRepository.ObterPorId(request.Id, cancellationToken);
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
        return coletaRepository.ObterTodos(cancellationToken);
    }
}

public class ObterColetasPorCorpoHidricoHandler : IRequestHandler<QueryObterColetasPorCorpoHidrico, List<ColetaEntity>>
{
    private readonly IColetaRepository coletaRepository;

    public ObterColetasPorCorpoHidricoHandler(IColetaRepository coletaRepository)
    {
        this.coletaRepository = coletaRepository;
    }

    public Task<List<ColetaEntity>> Handle(
        QueryObterColetasPorCorpoHidrico request,
        CancellationToken cancellationToken
    )
    {
        return coletaRepository.ObterPorCorpoHidrico(request.CorpoHidricoId, cancellationToken);
    }
}

public class ObterColetasPorDataHandler : IRequestHandler<QueryObterColetasPorData, List<ColetaEntity>>
{
    private readonly IColetaRepository coletaRepository;

    public ObterColetasPorDataHandler(IColetaRepository coletaRepository)
    {
        this.coletaRepository = coletaRepository;
    }

    public Task<List<ColetaEntity>> Handle(
        QueryObterColetasPorData request,
        CancellationToken cancellationToken
    )
    {
        return coletaRepository.ObterPorData(request.Data, cancellationToken);
    }
}
