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

    public void Cadastrar(MedicoesEntity medicao, int coletaId)
    {
        ArgumentNullException.ThrowIfNull(medicao);
        if (!context.Coletas.Any(c => c.Id == coletaId))
            throw new KeyNotFoundException("Coleta não encontrada.");
        ValidarDuplicidade(coletaId, medicao.codigoMedicao);
        medicao.VincularColeta(coletaId);
        context.Medicoes.Add(medicao);
        Salvar();
    }

    public void Atualizar(MedicoesEntity medicao, int coletaId, int medicaoId)
    {
        ArgumentNullException.ThrowIfNull(medicao);
        var existente = Encontrar(coletaId, medicaoId);
        ValidarDuplicidade(coletaId, medicao.codigoMedicao, medicaoId);
        existente.Atualizar(medicao.codigoMedicao, medicao.valor, medicao.unidade,
            medicao.censurado, medicao.limite);
        Salvar();
    }

    public void Deletar(int coletaId, int medicaoId)
    {
        context.Medicoes.Remove(Encontrar(coletaId, medicaoId));
        context.SaveChanges();
    }

    public MedicoesEntity? ObterPorId(int coletaId, int medicaoId) =>
        context.Medicoes.AsNoTracking().SingleOrDefault(m => m.ColetaId == coletaId && m.Id == medicaoId);

    public List<MedicoesEntity> ObterPorColeta(int coletaId) =>
        context.Medicoes.AsNoTracking().Where(m => m.ColetaId == coletaId).OrderBy(m => m.Id).ToList();

    public List<MedicoesEntity> ObterTodos() =>
        context.Medicoes.AsNoTracking().OrderBy(m => m.Id).ToList();

    private MedicoesEntity Encontrar(int coletaId, int medicaoId) =>
        context.Medicoes.SingleOrDefault(m => m.ColetaId == coletaId && m.Id == medicaoId)
        ?? throw new KeyNotFoundException("Medição não encontrada para essa coleta.");

    private void ValidarDuplicidade(int coletaId, Medicao codigo, int? medicaoId = null)
    {
        if (context.Medicoes.Any(m => m.ColetaId == coletaId && m.codigoMedicao == codigo
            && (!medicaoId.HasValue || m.Id != medicaoId.Value)))
            throw new InvalidOperationException("Já existe uma medição desse tipo na coleta.");
    }

    private void Salvar()
    {
        try
        {
            context.SaveChanges();
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
