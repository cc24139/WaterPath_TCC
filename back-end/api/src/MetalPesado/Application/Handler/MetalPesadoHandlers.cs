using Application.Commands.MetalPesado;
using Application.Queries.MetalPesado;
using back_end.src.Domain.MetalPesado;
using MediatR;

namespace Application.Handler.MetalPesado;

public class CadastrarMetalPesadoHandler : IRequestHandler<CommandCadastrarMetalPesado, Unit>
{
    private readonly IMetalPesadoRepository metalPesadoRepository;

    public CadastrarMetalPesadoHandler(IMetalPesadoRepository metalPesadoRepository)
    {
        this.metalPesadoRepository = metalPesadoRepository;
    }

    public async Task<Unit> Handle(
        CommandCadastrarMetalPesado request,
        CancellationToken cancellationToken
    )
    {
        await metalPesadoRepository.Cadastrar(request.MetalPesado, cancellationToken);
        return Unit.Value;
    }
}

public class AtualizarMetalPesadoHandler : IRequestHandler<CommandAtualizarMetalPesado, Unit>
{
    private readonly IMetalPesadoRepository metalPesadoRepository;

    public AtualizarMetalPesadoHandler(IMetalPesadoRepository metalPesadoRepository)
    {
        this.metalPesadoRepository = metalPesadoRepository;
    }

    public async Task<Unit> Handle(
        CommandAtualizarMetalPesado request,
        CancellationToken cancellationToken
    )
    {
        await metalPesadoRepository.Atualizar(request.MetalPesado, cancellationToken);
        return Unit.Value;
    }
}

public class DeletarMetalPesadoHandler : IRequestHandler<CommandDeletarMetalPesado, Unit>
{
    private readonly IMetalPesadoRepository metalPesadoRepository;

    public DeletarMetalPesadoHandler(IMetalPesadoRepository metalPesadoRepository)
    {
        this.metalPesadoRepository = metalPesadoRepository;
    }

    public async Task<Unit> Handle(CommandDeletarMetalPesado request, CancellationToken cancellationToken)
    {
        await metalPesadoRepository.Deletar(request.Id, cancellationToken);
        return Unit.Value;
    }
}

public class ObterMetalPesadoPorIdHandler
    : IRequestHandler<QueryObterMetalPesadoPorId, MetalPesadoEntity?>
{
    private readonly IMetalPesadoRepository metalPesadoRepository;

    public ObterMetalPesadoPorIdHandler(IMetalPesadoRepository metalPesadoRepository)
    {
        this.metalPesadoRepository = metalPesadoRepository;
    }

    public Task<MetalPesadoEntity?> Handle(
        QueryObterMetalPesadoPorId request,
        CancellationToken cancellationToken
    )
    {
        return metalPesadoRepository.ObterPorId(request.Id, cancellationToken);
    }
}

public class ObterTodosMetaisPesadosHandler
    : IRequestHandler<QueryObterTodosMetaisPesados, List<MetalPesadoEntity>>
{
    private readonly IMetalPesadoRepository metalPesadoRepository;

    public ObterTodosMetaisPesadosHandler(IMetalPesadoRepository metalPesadoRepository)
    {
        this.metalPesadoRepository = metalPesadoRepository;
    }

    public Task<List<MetalPesadoEntity>> Handle(
        QueryObterTodosMetaisPesados request,
        CancellationToken cancellationToken
    )
    {
        return metalPesadoRepository.ObterTodos(cancellationToken);
    }
}
