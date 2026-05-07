using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.User;
using Infrastructure.Data;

namespace back_end.src.Infrastructure.Repository
{


    public class UserRepository : IUserRepository
    {
        private readonly WaterPathDbContext context;

        public UserRepository(WaterPathDbContext context)
        {
            this.context = context;
        }
        public void Atualizar(UserEntity user)
        {
            var userToUpdate = context.Usuarios.Find(user.Id);
            if (userToUpdate == null)
            {
                throw new ArgumentException("Usuário não encontrado");
            }
            userToUpdate = new UserEntity(user.Nome, user.Senha, user.Email);
            context.Usuarios.Update(userToUpdate);
            context.SaveChanges();
        }

        public void Cadastrar(UserEntity user)
        {
            context.Usuarios.Add(user);
            context.SaveChanges();
        }

        public void Deletar(int id)
        {
            var userToDelete = context.Usuarios.Find(id);
            if (userToDelete == null)
            {
                throw new ArgumentException("Usuário não encontrado");
            }
            context.Usuarios.Remove(userToDelete);
            context.SaveChanges();
        }

        public List<UserEntity> ObterTodos()
        {
            return context.Usuarios.ToList();
        }

        public UserEntity ObterUsuarioPorEmail(string email)
        {
            return context.Usuarios.FirstOrDefault(u => u.Email == email);
        }
        

        public UserEntity ObterUsuarioPorId(int id)
        {
            return context.Usuarios.FirstOrDefault(u => u.Id == id);
        }
    }
}