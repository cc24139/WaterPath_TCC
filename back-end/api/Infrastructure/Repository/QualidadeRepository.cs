using System;
using System.Collections.Generic;
using System.Linq;
using back_end.src.Domain.Qualidade;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace back_end.src.Infrastructure.Repository
{
    public class QualidadeRepository : IQualidadeRepository
    {
        private readonly WaterPathDbContext context;

        public QualidadeRepository(WaterPathDbContext context)
        {
            this.context = context;
        }

        public async Task Atualizar(int id, QualidadeEntity qualidade, CancellationToken cancellationToken = default)
        {
            var entityToUpdate = await context.Qualidades
                .Include(q => q.Medicoes)
                .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
            if (entityToUpdate == null)
            {
                throw new ArgumentException("Qualidade não encontrada");
            }

            if (qualidade.MedicoesInformadas)
            {
                context.Medicoes.RemoveRange(entityToUpdate.Medicoes);
                entityToUpdate.Medicoes.Clear();
                entityToUpdate.Medicoes.AddRange(qualidade.Medicoes);
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task Cadastrar(QualidadeEntity qualidade, CancellationToken cancellationToken = default)
        {
            await context.Qualidades.AddAsync(qualidade, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task Deletar(int id, CancellationToken cancellationToken = default)
        {
            var entityToDelete = await context.Qualidades.FindAsync([id], cancellationToken);
            if (entityToDelete == null)
            {
                throw new ArgumentException("Qualidade não encontrada");
            }
            context.Qualidades.Remove(entityToDelete);
            await context.SaveChangesAsync(cancellationToken);
        }

        public Task<List<QualidadeEntity>> ObterTodos(CancellationToken cancellationToken = default)
        {
            return context.Qualidades.AsNoTracking()
                .Include(q => q.CorpoHidrico)
                .Include(q => q.QualidadeFutura)
                .Include(q => q.Medicoes)
                .ToListAsync(cancellationToken);
        }

        public Task<QualidadeEntity?> ObterPorId(int id, CancellationToken cancellationToken = default)
        {
            return context.Qualidades
                .Include(q => q.Medicoes)
                .FirstOrDefaultAsync(q => q.Id == id, cancellationToken);
        }
    }
}
