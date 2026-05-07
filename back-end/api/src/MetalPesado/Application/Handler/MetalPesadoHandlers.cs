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

    public Task<Unit> Handle(
        CommandCadastrarMetalPesado request,
        CancellationToken cancellationToken
    )
    {
        metalPesadoRepository.Cadastrar(request.MetalPesado);
        return Task.FromResult(Unit.Value);
    }
}

public class AtualizarMetalPesadoHandler : IRequestHandler<CommandAtualizarMetalPesado, Unit>
{
    private readonly IMetalPesadoRepository metalPesadoRepository;

    public AtualizarMetalPesadoHandler(IMetalPesadoRepository metalPesadoRepository)
    {
        this.metalPesadoRepository = metalPesadoRepository;
    }

    public Task<Unit> Handle(
        CommandAtualizarMetalPesado request,
        CancellationToken cancellationToken
    )
    {
        metalPesadoRepository.Atualizar(request.MetalPesado);
        return Task.FromResult(Unit.Value);
    }
}

public class DeletarMetalPesadoHandler : IRequestHandler<CommandDeletarMetalPesado, Unit>
{
    private readonly IMetalPesadoRepository metalPesadoRepository;

    public DeletarMetalPesadoHandler(IMetalPesadoRepository metalPesadoRepository)
    {
        this.metalPesadoRepository = metalPesadoRepository;
    }

    public Task<Unit> Handle(CommandDeletarMetalPesado request, CancellationToken cancellationToken)
    {
        metalPesadoRepository.Deletar(request.Id);
        return Task.FromResult(Unit.Value);
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
        var metalPesado = metalPesadoRepository.ObterPorId(request.Id);
        return Task.FromResult(metalPesado);
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
        var metaisPesados = metalPesadoRepository.ObterTodos();
        return Task.FromResult(metaisPesados);
    }
}
