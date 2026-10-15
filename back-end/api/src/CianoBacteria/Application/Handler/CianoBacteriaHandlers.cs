using Application.Commands.CianoBacteria;
using Application.Queries.CianoBacteria;
using back_end.src.Domain.CianoBacteria;
using MediatR;

namespace Application.Handler.CianoBacteria;

public class CadastrarCianoBacteriaHandler : IRequestHandler<CommandCadastrarCianoBacteria, Unit>
{
    private readonly ICianoBacteriaRepository cianoBacteriaRepository;

    public CadastrarCianoBacteriaHandler(ICianoBacteriaRepository cianoBacteriaRepository)
    {
        this.cianoBacteriaRepository = cianoBacteriaRepository;
    }

    public async Task<Unit> Handle(
        CommandCadastrarCianoBacteria request,
        CancellationToken cancellationToken
    )
    {
        await cianoBacteriaRepository.Cadastrar(request.CianoBacteria, cancellationToken);
        return Unit.Value;
    }
}

public class AtualizarCianoBacteriaHandler : IRequestHandler<CommandAtualizarCianoBacteria, Unit>
{
    private readonly ICianoBacteriaRepository cianoBacteriaRepository;

    public AtualizarCianoBacteriaHandler(ICianoBacteriaRepository cianoBacteriaRepository)
    {
        this.cianoBacteriaRepository = cianoBacteriaRepository;
    }

    public async Task<Unit> Handle(
        CommandAtualizarCianoBacteria request,
        CancellationToken cancellationToken
    )
    {
        await cianoBacteriaRepository.Atualizar(request.CianoBacteria, cancellationToken);
        return Unit.Value;
    }
}

public class DeletarCianoBacteriaHandler : IRequestHandler<CommandDeletarCianoBacteria, Unit>
{
    private readonly ICianoBacteriaRepository cianoBacteriaRepository;

    public DeletarCianoBacteriaHandler(ICianoBacteriaRepository cianoBacteriaRepository)
    {
        this.cianoBacteriaRepository = cianoBacteriaRepository;
    }

    public async Task<Unit> Handle(
        CommandDeletarCianoBacteria request,
        CancellationToken cancellationToken
    )
    {
        await cianoBacteriaRepository.Deletar(request.Id, cancellationToken);
        return Unit.Value;
    }
}

public class ObterCianoBacteriaPorIdHandler
    : IRequestHandler<QueryObterCianoBacteriaPorId, CianoBacteriaEntity?>
{
    private readonly ICianoBacteriaRepository cianoBacteriaRepository;

    public ObterCianoBacteriaPorIdHandler(ICianoBacteriaRepository cianoBacteriaRepository)
    {
        this.cianoBacteriaRepository = cianoBacteriaRepository;
    }

    public Task<CianoBacteriaEntity?> Handle(
        QueryObterCianoBacteriaPorId request,
        CancellationToken cancellationToken
    )
    {
        return cianoBacteriaRepository.ObterPorId(request.Id, cancellationToken);
    }
}

public class ObterTodasCianoBacteriasHandler
    : IRequestHandler<QueryObterTodasCianoBacterias, List<CianoBacteriaEntity>>
{
    private readonly ICianoBacteriaRepository cianoBacteriaRepository;

    public ObterTodasCianoBacteriasHandler(ICianoBacteriaRepository cianoBacteriaRepository)
    {
        this.cianoBacteriaRepository = cianoBacteriaRepository;
    }

    public Task<List<CianoBacteriaEntity>> Handle(
        QueryObterTodasCianoBacterias request,
        CancellationToken cancellationToken
    )
    {
        return cianoBacteriaRepository.ObterTodos(cancellationToken);
    }
}
