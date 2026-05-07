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

        public void Atualizar(CorpoHidricoEntity corpoHidrico)
        {
            var entityToUpdate = context.CorposHidricos.Find(corpoHidrico.Id);
            if (entityToUpdate == null)
            {
                throw new ArgumentException("Corpo hídrico não encontrado");
            }
            context.CorposHidricos.Update(corpoHidrico);
            context.SaveChanges();
        }

        public void Cadastrar(CorpoHidricoEntity corpoHidrico)
        {
            context.CorposHidricos.Add(corpoHidrico);
            context.SaveChanges();
        }

        public void Deletar(int id)
        {
            var entityToDelete = context.CorposHidricos.Find(id);
            if (entityToDelete == null)
            {
                throw new ArgumentException("Corpo hídrico não encontrado");
            }
            context.CorposHidricos.Remove(entityToDelete);
            context.SaveChanges();
        }

        public List<CorpoHidricoEntity> ObterTodos()
        {
            return context.CorposHidricos.ToList();
        }

        public CorpoHidricoEntity? ObterCorpoHidricoPorId(int id)
        {
            return context.CorposHidricos.FirstOrDefault(c => c.Id == id);
        }

        public List<CorpoHidricoEntity> ObterCorposHidricosPorUsuario(int userId)
        {
            return context.CorposHidricos.Where(c => c.users.Any(u => u.Id == userId)).ToList();
        }

        public List<int> ObterUsuariosDoCorpoHidrico(int corpoHidricoId)
        {
            return context
                .Usuarios.Where(u => u.CorpoHidricos.Any(c => c.Id == corpoHidricoId))
                .Select(u => u.Id)
                .ToList();
        }

        public List<int> ObterColetasDoCorpoHidrico(int corpoHidricoId)
        {
            return context
                .Coletas.Where(c => c.CorpoHidrico.Id == corpoHidricoId)
                .Select(c => c.Id)
                .ToList();
        }

        public void AdicionarUsuario(int corpoHidricoId, int userId)
        {
            var corpoHidrico = context
                .CorposHidricos.Include(c => c.users)
                .FirstOrDefault(c => c.Id == corpoHidricoId);

            if (corpoHidrico == null)
            {
                throw new ArgumentException("Corpo hídrico não encontrado");
            }

            var usuario = context.Usuarios.Find(userId);
            if (usuario == null)
            {
                throw new ArgumentException("Usuário não encontrado");
            }

            if (corpoHidrico.users.Any(u => u.Id == userId))
            {
                return;
            }

            corpoHidrico.users.Add(usuario);
            context.SaveChanges();
        }

        public void DeletarUsuario(int corpoHidricoId, int userId)
        {
            var corpoHidrico = context
                .CorposHidricos.Include(c => c.users)
                .FirstOrDefault(c => c.Id == corpoHidricoId);

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
            context.SaveChanges();
        }
    }
}
