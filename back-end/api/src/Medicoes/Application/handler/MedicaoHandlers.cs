using back_end.src.Medicoes.Domain;
using back_end.src.Medicoes.Application.Queries;
using MediatR;

namespace back_end.src.Medicoes.Application.Handler;

public class CreateMedicaoHandler : IRequestHandler<CreateMedicaoCommand, Unit>
{
    private readonly IMedicoesRepository medicoesRepository;

    public CreateMedicaoHandler(IMedicoesRepository medicoesRepository)
    {
        this.medicoesRepository = medicoesRepository;
    }

    public Task<Unit> Handle(CreateMedicaoCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        MedicaoHandlerSupport.ValidarIds(request.ColetaId);
        var medicao = MedicaoHandlerSupport.CriarMedicao(request.medicao);
        medicoesRepository.Cadastrar(medicao, request.ColetaId);
        return Task.FromResult(Unit.Value);
    }
}

public class UpdateMedicaoHandler : IRequestHandler<UpdateMedicaoCommand, Unit>
{
    private readonly IMedicoesRepository medicoesRepository;

    public UpdateMedicaoHandler(IMedicoesRepository medicoesRepository)
    {
        this.medicoesRepository = medicoesRepository;
    }

    public Task<Unit> Handle(UpdateMedicaoCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        MedicaoHandlerSupport.ValidarIds(request.ColetaId, request.MedicaoId);
        var medicao = MedicaoHandlerSupport.CriarMedicao(request.Medicao);
        medicoesRepository.Atualizar(medicao, request.ColetaId, request.MedicaoId);
        return Task.FromResult(Unit.Value);
    }
}

public class DeleteMedicaoHandler : IRequestHandler<DeleteMedicaoCommand, Unit>
{
    private readonly IMedicoesRepository medicoesRepository;

    public DeleteMedicaoHandler(IMedicoesRepository medicoesRepository)
    {
        this.medicoesRepository = medicoesRepository;
    }

    public Task<Unit> Handle(DeleteMedicaoCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        MedicaoHandlerSupport.ValidarIds(request.ColetaId, request.MedicaoId);
        medicoesRepository.Deletar(request.ColetaId, request.MedicaoId);
        return Task.FromResult(Unit.Value);
    }
}

public class ObterMedicaoPorIdHandler : IRequestHandler<QueryObterMedicaoPorId, MedicoesEntity?>
{
    private readonly IMedicoesRepository medicoesRepository;

    public ObterMedicaoPorIdHandler(IMedicoesRepository medicoesRepository)
    {
        this.medicoesRepository = medicoesRepository;
    }

    public Task<MedicoesEntity?> Handle(QueryObterMedicaoPorId request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        MedicaoHandlerSupport.ValidarIds(request.ColetaId, request.MedicaoId);
        return Task.FromResult(medicoesRepository.ObterPorId(request.ColetaId, request.MedicaoId));
    }
}

public class ObterMedicoesPorColetaHandler : IRequestHandler<QueryObterMedicoesPorColeta, List<MedicoesEntity>>
{
    private readonly IMedicoesRepository medicoesRepository;

    public ObterMedicoesPorColetaHandler(IMedicoesRepository medicoesRepository)
    {
        this.medicoesRepository = medicoesRepository;
    }

    public Task<List<MedicoesEntity>> Handle(QueryObterMedicoesPorColeta request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        MedicaoHandlerSupport.ValidarIds(request.ColetaId);
        return Task.FromResult(medicoesRepository.ObterPorColeta(request.ColetaId));
    }
}

public class ObterTodasMedicoesHandler : IRequestHandler<QueryObterTodasMedicoes, List<MedicoesEntity>>
{
    private readonly IMedicoesRepository medicoesRepository;

    public ObterTodasMedicoesHandler(IMedicoesRepository medicoesRepository)
    {
        this.medicoesRepository = medicoesRepository;
    }

    public Task<List<MedicoesEntity>> Handle(QueryObterTodasMedicoes request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(medicoesRepository.ObterTodos());
    }
}

internal static class MedicaoHandlerSupport
{
    internal static void ValidarIds(int coletaId, int? medicaoId = null)
    {
        if (coletaId <= 0)
            throw new ArgumentException("Id da coleta inválido.");
        if (medicaoId is <= 0)
            throw new ArgumentException("Id da medição inválido.");
    }

    internal static MedicoesEntity CriarMedicao(InputMedicao input)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (!Enum.TryParse<Medicao>(input.codigoMedicao, true, out var codigo)
            || !Enum.IsDefined(codigo))
            throw new ArgumentException("Código de medição inválido.");
        return new MedicoesEntity(codigo, input.valor, input.unidade, input.ehCensurado, input.limite);
    }
}
