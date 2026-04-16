using System;
using Domain.User;

namespace back_end.src.Domain.Codigo
{
    public class CodigoEntity
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public DateTime DataGeracao { get; set; }
        public DateTime DataExpiracao { get; set; }
        public bool Usado { get; set; }
        public int UsuarioId { get; set; }
        public UserEntity Usuario { get; set; }

        public CodigoEntity() { }

        public CodigoEntity(UserEntity usuario)
        {
            Codigo = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            Usuario = usuario;
            UsuarioId = usuario.Id;
            DataGeracao = DateTime.UtcNow;
            DataExpiracao = DateTime.UtcNow.AddMinutes(15);
            Usado = false;
        }
    }
}
