using Application.Commands.Qualidade;
using Application.Queries.Qualidade;
using back_end.src.Domain.Qualidade;
using back_end.src.Medicoes.Domain;
using MediatR;

namespace Application.Handler.Qualidade;

public class CadastrarQualidadeHandler : IRequestHandler<CommandCadastrarQualidade, Unit>
{
    private readonly IQualidadeRepository qualidadeRepository;

    public CadastrarQualidadeHandler(IQualidadeRepository qualidadeRepository)
    {
        this.qualidadeRepository = qualidadeRepository;
    }

    public async Task<Unit> Handle(CommandCadastrarQualidade request, CancellationToken cancellationToken)
    {
        await qualidadeRepository.Cadastrar(request.Qualidade, cancellationToken);
        return Unit.Value;
    }
}

public class AtualizarQualidadeHandler : IRequestHandler<CommandAtualizarQualidade, Unit>
{
    private readonly IQualidadeRepository qualidadeRepository;

    public AtualizarQualidadeHandler(IQualidadeRepository qualidadeRepository)
    {
        this.qualidadeRepository = qualidadeRepository;
    }

    public async Task<Unit> Handle(CommandAtualizarQualidade request, CancellationToken cancellationToken)
    {
        await qualidadeRepository.Atualizar(request.Id, request.Qualidade, cancellationToken);
        return Unit.Value;
    }
}

public class DeletarQualidadeHandler : IRequestHandler<CommandDeletarQualidade, Unit>
{
    private readonly IQualidadeRepository qualidadeRepository;

    public DeletarQualidadeHandler(IQualidadeRepository qualidadeRepository)
    {
        this.qualidadeRepository = qualidadeRepository;
    }

    public async Task<Unit> Handle(CommandDeletarQualidade request, CancellationToken cancellationToken)
    {
        await qualidadeRepository.Deletar(request.Id, cancellationToken);
        return Unit.Value;
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
        return qualidadeRepository.ObterPorId(request.Id, cancellationToken);
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
        return qualidadeRepository.ObterTodos(cancellationToken);
    }
}

public class ObterMedicaoDaQualidadeHandler
    : IRequestHandler<QueryObterMedicaoDaQualidade, List<MedicoesEntity>>
{
    private readonly IQualidadeRepository qualidadeRepository;

    public ObterMedicaoDaQualidadeHandler(IQualidadeRepository qualidadeRepository)
    {
        this.qualidadeRepository = qualidadeRepository;
    }

    public async Task<List<MedicoesEntity>> Handle(
        QueryObterMedicaoDaQualidade request,
        CancellationToken cancellationToken
    )
    {
        var qualidade = await qualidadeRepository.ObterPorId(request.Id, cancellationToken);
        return qualidade?.Medicoes?.Where(m => m.codigoMedicao == request.CodigoMedicao).ToList() ?? [];
    }
}
