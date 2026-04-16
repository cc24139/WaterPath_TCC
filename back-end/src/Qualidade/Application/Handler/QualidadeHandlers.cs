using Application.Commands.Qualidade;
using Application.Queries.Qualidade;
using back_end.src.Domain.Qualidade;
using MediatR;

namespace Application.Handler.Qualidade;

public class CadastrarQualidadeHandler : IRequestHandler<CommandCadastrarQualidade, Unit>
{
    private readonly IQualidadeRepository qualidadeRepository;

    public CadastrarQualidadeHandler(IQualidadeRepository qualidadeRepository)
    {
        this.qualidadeRepository = qualidadeRepository;
    }

    public Task<Unit> Handle(CommandCadastrarQualidade request, CancellationToken cancellationToken)
    {
        qualidadeRepository.Cadastrar(request.Qualidade);
        return Task.FromResult(Unit.Value);
    }
}

public class AtualizarQualidadeHandler : IRequestHandler<CommandAtualizarQualidade, Unit>
{
    private readonly IQualidadeRepository qualidadeRepository;

    public AtualizarQualidadeHandler(IQualidadeRepository qualidadeRepository)
    {
        this.qualidadeRepository = qualidadeRepository;
    }

    public Task<Unit> Handle(CommandAtualizarQualidade request, CancellationToken cancellationToken)
    {
        qualidadeRepository.Atualizar(request.Qualidade);
        return Task.FromResult(Unit.Value);
    }
}

public class DeletarQualidadeHandler : IRequestHandler<CommandDeletarQualidade, Unit>
{
    private readonly IQualidadeRepository qualidadeRepository;

    public DeletarQualidadeHandler(IQualidadeRepository qualidadeRepository)
    {
        this.qualidadeRepository = qualidadeRepository;
    }

    public Task<Unit> Handle(CommandDeletarQualidade request, CancellationToken cancellationToken)
    {
        qualidadeRepository.Deletar(request.Id);
        return Task.FromResult(Unit.Value);
    }
}

public class ObterQualidadePorIdHandler
    : IRequestHandler<QueryObterQualidadePorId, QualidadeEntity?>
{
    private readonly IQualidadeRepository qualidadeRepository;

    public ObterQualidadePorIdHandler(IQualidadeRepository qualidadeRepository)
    {
        this.qualidadeRepository = qualidadeRepository;
    }

    public Task<QualidadeEntity?> Handle(
        QueryObterQualidadePorId request,
        CancellationToken cancellationToken
    )
    {
        var qualidade = qualidadeRepository.ObterPorId(request.Id);
        return Task.FromResult(qualidade);
    }
}

public class ObterTodasQualidadesHandler
    : IRequestHandler<QueryObterTodasQualidades, List<QualidadeEntity>>
{
    private readonly IQualidadeRepository qualidadeRepository;

    public ObterTodasQualidadesHandler(IQualidadeRepository qualidadeRepository)
    {
        this.qualidadeRepository = qualidadeRepository;
    }

    public Task<List<QualidadeEntity>> Handle(
        QueryObterTodasQualidades request,
        CancellationToken cancellationToken
    )
    {
        var qualidades = qualidadeRepository.ObterTodos();
        return Task.FromResult(qualidades);
    }
}
