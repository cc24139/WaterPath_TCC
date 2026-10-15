using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.User;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace back_end.src.Infrastructure.Repository
{


    public class UserRepository : IUserRepository
    {
        private readonly WaterPathDbContext context;

        public UserRepository(WaterPathDbContext context)
        {
            this.context = context;
        }
        public async Task Atualizar(UserEntity user, CancellationToken cancellationToken = default)
        {
            var userToUpdate = await context.Usuarios.FindAsync([user.Id], cancellationToken);
            if (userToUpdate == null)
            {
                throw new ArgumentException("Usuário não encontrado");
            }
            context.Entry(userToUpdate).CurrentValues.SetValues(user);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task Cadastrar(UserEntity user, CancellationToken cancellationToken = default)
        {
            await context.Usuarios.AddAsync(user, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task Deletar(int id, CancellationToken cancellationToken = default)
        {
            var userToDelete = await context.Usuarios.FindAsync([id], cancellationToken);
            if (userToDelete == null)
            {
                throw new ArgumentException("Usuário não encontrado");
            }
            context.Usuarios.Remove(userToDelete);
            await context.SaveChangesAsync(cancellationToken);
        }

        public Task<List<UserEntity>> ObterTodos(CancellationToken cancellationToken = default)
        {
            return context.Usuarios.ToListAsync(cancellationToken);
        }

        public Task<UserEntity?> ObterUsuarioPorEmail(string email, CancellationToken cancellationToken = default)
        {
            return context.Usuarios.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
        }
        

        public Task<UserEntity?> ObterUsuarioPorId(int id, CancellationToken cancellationToken = default)
        {
            return context.Usuarios.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
        }

        public Task<List<UserEntity>> ObterUsuariosPorIds(List<int> ids, CancellationToken cancellationToken = default)
        {
            return context.Usuarios.Where(u => ids.Contains(u.Id)).ToListAsync(cancellationToken);
        }

        public async Task AlterarSenha(string email, string senha, CancellationToken cancellationToken = default)
        {
            var userToUpdate = await context.Usuarios.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
            if (userToUpdate == null)
            {
                throw new ArgumentException("Usuário não encontrado");
            }
            userToUpdate.Senha = senha;
            context.Usuarios.Update(userToUpdate);
            await context.SaveChangesAsync(cancellationToken);
        }
    }
}
