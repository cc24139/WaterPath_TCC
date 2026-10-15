using back_end.src.Medicoes.Domain;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace back_end.src.Infrastructure.Repository;

public class MedicoesRepository : IMedicoesRepository
{
    private readonly WaterPathDbContext context;

    public MedicoesRepository(WaterPathDbContext context)
    {
        this.context = context;
    }

    public async Task Cadastrar(MedicoesEntity medicao, int coletaId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(medicao);
        if (!await context.Coletas.AnyAsync(c => c.Id == coletaId, cancellationToken))
            throw new KeyNotFoundException("Coleta não encontrada.");
        await ValidarDuplicidade(coletaId, medicao.codigoMedicao, cancellationToken: cancellationToken);
        medicao.VincularColeta(coletaId);
        await context.Medicoes.AddAsync(medicao, cancellationToken);
        await Salvar(cancellationToken);
    }

    public async Task Atualizar(MedicoesEntity medicao, int coletaId, int medicaoId, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(medicao);
        var existente = await Encontrar(coletaId, medicaoId, cancellationToken);
        await ValidarDuplicidade(coletaId, medicao.codigoMedicao, medicaoId, cancellationToken);
        existente.Atualizar(medicao.codigoMedicao, medicao.valor, medicao.unidade,
            medicao.censurado, medicao.limite);
        await Salvar(cancellationToken);
    }

    public async Task Deletar(int coletaId, int medicaoId, CancellationToken cancellationToken = default)
    {
        context.Medicoes.Remove(await Encontrar(coletaId, medicaoId, cancellationToken));
        await context.SaveChangesAsync(cancellationToken);
    }

    public Task<MedicoesEntity?> ObterPorId(int coletaId, int medicaoId, CancellationToken cancellationToken = default) =>
        context.Medicoes.AsNoTracking().SingleOrDefaultAsync(m => m.ColetaId == coletaId && m.Id == medicaoId, cancellationToken);

    public Task<List<MedicoesEntity>> ObterPorColeta(int coletaId, CancellationToken cancellationToken = default) =>
        context.Medicoes.AsNoTracking().Where(m => m.ColetaId == coletaId).OrderBy(m => m.Id).ToListAsync(cancellationToken);

    public Task<List<MedicoesEntity>> ObterTodos(CancellationToken cancellationToken = default) =>
        context.Medicoes.AsNoTracking().OrderBy(m => m.Id).ToListAsync(cancellationToken);
    
    public Task<List<MedicoesEntity>> ObterPorCodigo(int coletaId, Medicao codigo, CancellationToken cancellationToken = default) =>
        context.Medicoes.AsNoTracking().Where(m => m.ColetaId == coletaId && m.codigoMedicao == codigo).ToListAsync(cancellationToken);
    
    public Task<List<MedicoesEntity>> ObterPorPeriodo(int coletaId, DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default) =>
        context.Medicoes.AsNoTracking()
            .Where(m => m.ColetaId == coletaId && m.Coleta.DataHora >= dataInicio && m.Coleta.DataHora <= dataFim)
            .OrderBy(m => m.Coleta.DataHora)
            .ToListAsync(cancellationToken);
    
    public Task<List<MedicoesEntity>> ObterPorCodigoEPeriodo(int coletaId, Medicao codigo, DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default) =>
        context.Medicoes.AsNoTracking()
            .Where(m => m.ColetaId == coletaId && m.codigoMedicao == codigo && m.Coleta.DataHora >= dataInicio && m.Coleta.DataHora <= dataFim)
            .OrderBy(m => m.Coleta.DataHora)
            .ToListAsync(cancellationToken);
    
    

    private async Task<MedicoesEntity> Encontrar(int coletaId, int medicaoId, CancellationToken cancellationToken) =>
        await context.Medicoes.SingleOrDefaultAsync(m => m.ColetaId == coletaId && m.Id == medicaoId, cancellationToken)
        ?? throw new KeyNotFoundException("Medição não encontrada para essa coleta.");

    private async Task ValidarDuplicidade(int coletaId, Medicao codigo, int? medicaoId = null, CancellationToken cancellationToken = default)
    {
        if (await context.Medicoes.AnyAsync(m => m.ColetaId == coletaId && m.codigoMedicao == codigo
            && (!medicaoId.HasValue || m.Id != medicaoId.Value), cancellationToken))
            throw new InvalidOperationException("Já existe uma medição desse tipo na coleta.");
    }

    private async Task Salvar(CancellationToken cancellationToken)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: "IX_Medicoes_ColetaId_codigoMedicao"
        })
        {
            throw new InvalidOperationException("Já existe uma medição desse tipo na coleta.", ex);
        }
    }
}
