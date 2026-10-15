using System;
using System.Collections.Generic;
using System.Linq;
using back_end.src.Domain.CorpoHidrico;
using Domain.User;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace back_end.src.Infrastructure.Repository
{
    public class CorpoHidricoRepository : ICorpoHidricoRepository
    {
        private readonly WaterPathDbContext context;

        public CorpoHidricoRepository(WaterPathDbContext context)
        {
            this.context = context;
        }

        public async Task Atualizar(CorpoHidricoEntity corpoHidrico, CancellationToken cancellationToken = default)
        {
            var entityToUpdate = await context.CorposHidricos.FindAsync([corpoHidrico.Id], cancellationToken);
            if (entityToUpdate == null)
            {
                throw new ArgumentException("Corpo hídrico não encontrado");
            }
            context.CorposHidricos.Update(corpoHidrico);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task Cadastrar(CorpoHidricoEntity corpoHidrico, CancellationToken cancellationToken = default)
        {
            await context.CorposHidricos.AddAsync(corpoHidrico, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            var usuarios = corpoHidrico.users;
            foreach (var usuario in usuarios)
            {
                var userEntity = await context.Usuarios.FindAsync([usuario.Id], cancellationToken);
                if (userEntity != null)
                {
                    userEntity.CorpoHidricos.Add(corpoHidrico);
                    context.CorposHidricos.Update(corpoHidrico);
                    context.Usuarios.Update(userEntity);
                }
            }
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task Deletar(int id, CancellationToken cancellationToken = default)
        {
            var entityToDelete = await context.CorposHidricos.FindAsync([id], cancellationToken);
            if (entityToDelete == null)
            {
                throw new ArgumentException("Corpo hídrico não encontrado");
            }
            context.CorposHidricos.Remove(entityToDelete);
            await context.SaveChangesAsync(cancellationToken);
        }

        public Task<List<CorpoHidricoEntity>> ObterTodos(CancellationToken cancellationToken = default)
        {
            return context.CorposHidricos.Include(c => c.users).ToListAsync(cancellationToken);
        }

        public Task<CorpoHidricoEntity?> ObterCorpoHidricoPorId(int id, CancellationToken cancellationToken = default)
        {
            return context.CorposHidricos.Include(c => c.users).FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public Task<List<CorpoHidricoEntity>> ObterCorposHidricosPorUsuario(int userId, CancellationToken cancellationToken = default)
        {
            return context
                .CorposHidricos.Include(c => c.users)
                .Where(c => c.users.Any(u => u.Id == userId))
                .ToListAsync(cancellationToken);
        }

        public Task<List<int>> ObterUsuariosDoCorpoHidrico(int corpoHidricoId, CancellationToken cancellationToken = default)
        {
            return context
                .Usuarios.Where(u => u.CorpoHidricos.Any(c => c.Id == corpoHidricoId))
                .Select(u => u.Id)
                .ToListAsync(cancellationToken);
        }

        public Task<List<int>> ObterColetasDoCorpoHidrico(int corpoHidricoId, CancellationToken cancellationToken = default)
        {
            return context
                .Coletas.Where(c => c.CorpoHidricoId == corpoHidricoId)
                .Select(c => c.Id)
                .ToListAsync(cancellationToken);
        }

        public IAsyncEnumerable<ResultadoRiscoPersistido> ObterResultadosRisco(int corpoHidricoId)
        {
            return context.PredicoesIA.AsNoTracking()
                .Where(p => p.CorpoHidricoId == corpoHidricoId && p.DataColeta != null
                    && p.DataColeta <= DateTime.UtcNow
                    && p.Tipo == "integrada" && p.ResultadoJson != null)
                .OrderByDescending(p => p.DataColeta).ThenByDescending(p => p.ColetaId)
                .ThenByDescending(p => p.CriadaEm).ThenByDescending(p => p.Id)
                .Select(p => new ResultadoRiscoPersistido(
                    p.Id, p.ColetaId, new DateTimeOffset(p.DataColeta!.Value, TimeSpan.Zero), p.CriadaEm, p.ResultadoJson!, p.EntradaJson))
                .AsAsyncEnumerable();
        }

        public async Task AdicionarUsuario(int corpoHidricoId, int userId, CancellationToken cancellationToken = default)
        {
            var corpoHidrico = await context
                .CorposHidricos.Include(c => c.users)
                .FirstOrDefaultAsync(c => c.Id == corpoHidricoId, cancellationToken);

            if (corpoHidrico == null)
            {
                throw new ArgumentException("Corpo hídrico não encontrado");
            }

            var usuario = await context.Usuarios.FindAsync([userId], cancellationToken);
            if (usuario == null)
            {
                throw new ArgumentException("Usuário não encontrado");
            }

            if (corpoHidrico.users.Any(u => u.Id == userId))
            {
                return;
            }

            corpoHidrico.users.Add(usuario);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeletarUsuario(int corpoHidricoId, int userId, CancellationToken cancellationToken = default)
        {
            var corpoHidrico = await context
                .CorposHidricos.Include(c => c.users)
                .FirstOrDefaultAsync(c => c.Id == corpoHidricoId, cancellationToken);

            if (corpoHidrico == null)
            {
                throw new ArgumentException("Corpo hídrico não encontrado");
            }

            var usuario = corpoHidrico.users.FirstOrDefault(u => u.Id == userId);
            if (usuario == null)
            {
                throw new ArgumentException("Usuário não está vinculado a este corpo hídrico");
            }

            corpoHidrico.users.Remove(usuario);
            await context.SaveChangesAsync(cancellationToken);
        }

        public Task<CorpoHidricoEntity?> ObterCorpoHidricoPorNome(string nome, CancellationToken cancellationToken = default)
        {
            return context.CorposHidricos.Include(c => c.users).FirstOrDefaultAsync(c => c.Nome == nome, cancellationToken);
        }
    }
}
