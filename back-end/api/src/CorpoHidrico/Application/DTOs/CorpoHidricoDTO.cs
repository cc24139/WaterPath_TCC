using System.Collections.Generic;
using System.Linq;
using Application.DTOs;
using back_end.src.Domain.CorpoHidrico;

namespace Application.DTOs
{
    public class CorpoHidricoDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Localizacao { get; set; }
        public double Tamanho { get; set; }
        public List<UserDTO> Users { get; set; }

        public static CorpoHidricoDTO FromEntity(CorpoHidricoEntity e)
        {
            return new CorpoHidricoDTO
            {
                Id = e.Id,
                Nome = e.Nome,
                Localizacao = e.Localizacao,
                Tamanho = e.Tamanho,
                Users =
                    e.users?.Select(u => new UserDTO
                        {
                            Id = u.Id,
                            Nome = u.Nome,
                            Email = u.Email,
                        })
                        .ToList()
                    ?? new List<UserDTO>(),
            };
        }

        public static List<CorpoHidricoDTO> FromEntities(List<CorpoHidricoEntity> list)
        {
            return list?.Select(FromEntity).ToList() ?? new List<CorpoHidricoDTO>();
        }
    }
}
