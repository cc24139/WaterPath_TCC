using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace back_end.src.IA.Application;

public class PredicaoIAAuthorizationService(WaterPathDbContext context)
{
    public Task<bool> PodeAnalisarColetaAsync(int coletaId, int usuarioId, CancellationToken cancellationToken) =>
        context.Coletas.AsNoTracking().AnyAsync(c => c.Id == coletaId
            && (c.ResponsavelId == usuarioId || c.CorpoHidrico.users.Any(u => u.Id == usuarioId)), cancellationToken);

    public Task<bool> PodeAnalisarCorpoHidricoAsync(int corpoHidricoId, int usuarioId,
        CancellationToken cancellationToken) =>
        context.CorposHidricos.AsNoTracking().AnyAsync(c => c.Id == corpoHidricoId
            && (c.users.Any(u => u.Id == usuarioId)
                || c.Coletas.Any(coleta => coleta.ResponsavelId == usuarioId)), cancellationToken);

    public IQueryable<back_end.src.IA.Domain.PredicaoIAEntity> FiltrarPredicoesDoUsuario(
        IQueryable<back_end.src.IA.Domain.PredicaoIAEntity> query, int usuarioId) =>
        query.Where(p => p.Coleta.ResponsavelId == usuarioId
            || p.Coleta.CorpoHidrico.users.Any(u => u.Id == usuarioId));
}
