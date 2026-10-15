using System;
using System.Collections.Generic;
using System.Linq;
using back_end.src.Domain.Coleta;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace back_end.src.Infrastructure.Repository
{
    public class ColetaRepository : IColetaRepository
    {
        private readonly WaterPathDbContext context;

        public ColetaRepository(WaterPathDbContext context)
        {
            this.context = context;
        }

        public virtual async Task Atualizar(ColetaEntity coleta, int idColeta, CancellationToken cancellationToken = default)
        {
            var entityToUpdate = await context.Coletas.Include(c => c.Medicoes)
                .SingleOrDefaultAsync(c => c.Id == idColeta, cancellationToken);
            if (entityToUpdate == null)
            {
                throw new ArgumentException("Coleta não encontrada");
            }
            await ValidarCorpoHidrico(coleta.CorpoHidricoId, cancellationToken);
            entityToUpdate.CorpoHidricoId = coleta.CorpoHidricoId;
            entityToUpdate.DataHora = coleta.DataHora.ToUniversalTime();
            entityToUpdate.Latitude = coleta.Latitude;
            entityToUpdate.Longitude = coleta.Longitude;
            entityToUpdate.ProfundidadeMetros = coleta.ProfundidadeMetros;
            // Medições omitidas são preservadas; exclusões usam o endpoint de medições.
            foreach (var medicao in coleta.Medicoes)
            {
                var existente = entityToUpdate.Medicoes.SingleOrDefault(m => m.codigoMedicao == medicao.codigoMedicao);
                if (existente is null)
                    entityToUpdate.AdicionarMedicao(medicao);
                else
                    existente.Atualizar(medicao.codigoMedicao, medicao.valor, medicao.unidade,
                        medicao.censurado, medicao.limite);
            }
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task Cadastrar(ColetaEntity coleta, int idCorpoHidrico, CancellationToken cancellationToken = default)
        {
            await ValidarCorpoHidrico(idCorpoHidrico, cancellationToken);
            coleta.CorpoHidricoId = idCorpoHidrico;
            coleta.DataHora = coleta.DataHora.ToUniversalTime();
            await context.Coletas.AddAsync(coleta, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task CadastrarListaColetas(List<ColetaEntity> coletas, CancellationToken cancellationToken = default)
        {
            foreach (var coleta in coletas)
            {
                await ValidarCorpoHidrico(coleta.CorpoHidricoId, cancellationToken);
                coleta.DataHora = coleta.DataHora.ToUniversalTime();
            }
            await context.Coletas.AddRangeAsync(coletas, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task Deletar(int id, CancellationToken cancellationToken = default)
        {
            var entityToDelete = await context.Coletas.FindAsync([id], cancellationToken);
            if (entityToDelete == null)
            {
                throw new ArgumentException("Coleta não encontrada");
            }
            context.Coletas.Remove(entityToDelete);
            await context.SaveChangesAsync(cancellationToken);
        }

        public Task<List<ColetaEntity>> ObterPorCorpoHidrico(int corpoHidricoId, CancellationToken cancellationToken = default)
        {
            return Consultar().Where(c => c.CorpoHidricoId == corpoHidricoId).ToListAsync(cancellationToken);
        }

        public Task<ColetaEntity?> ObterPorId(int id, CancellationToken cancellationToken = default)
        {
            return Consultar().SingleOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public Task<List<ColetaEntity>> ObterPorCorpoHidricoEPeriodo(int corpoHidricoId, string dataInicio, string dataFim, CancellationToken cancellationToken = default)
        {
            return Consultar()
                .Where(c =>
                    c.CorpoHidricoId == corpoHidricoId
                    && c.DataHora >= DateTimeOffset.Parse(dataInicio).ToUniversalTime()
                    && c.DataHora <= DateTimeOffset.Parse(dataFim).ToUniversalTime()
                )
                .ToListAsync(cancellationToken);
        }

        public Task<List<ColetaEntity>> ObterPorData(DateTime data, CancellationToken cancellationToken = default)
        {
            var dataInicio = new DateTimeOffset(data.Date, TimeSpan.Zero);
            var dataFim = new DateTimeOffset(data.Date.AddDays(1).AddTicks(-1), TimeSpan.Zero);
            return Consultar()
                .Where(c => c.DataHora >= dataInicio && c.DataHora <= dataFim)
                .ToListAsync(cancellationToken);
        }
        
        public Task<List<ColetaEntity>> ObterPorPeriodo(
            int corpoHidricoId,
            string dataInicio,
            string dataFim,
            CancellationToken cancellationToken = default
        )
        {
            return Consultar()
                .Where(c =>
                    c.CorpoHidricoId == corpoHidricoId
                    && c.DataHora >= DateTimeOffset.Parse(dataInicio).ToUniversalTime()
                    && c.DataHora<= DateTimeOffset.Parse(dataFim).ToUniversalTime()
                )
                .ToListAsync(cancellationToken);
        }

        private IQueryable<ColetaEntity> Consultar() => context.Coletas
            .AsNoTracking()
            .Include(c => c.Medicoes)
            .Include(c => c.MetaisPesados)
            .AsSplitQuery();

        private async Task ValidarCorpoHidrico(int id, CancellationToken cancellationToken)
        {
            if (id <= 0 || !await context.CorposHidricos.AnyAsync(c => c.Id == id, cancellationToken))
                throw new ArgumentException("Corpo Hídrico não encontrado");
        }

        public Task<List<ColetaEntity>> ObterTodos(CancellationToken cancellationToken = default)
        {
            return Consultar().ToListAsync(cancellationToken);
        }
    }
}
