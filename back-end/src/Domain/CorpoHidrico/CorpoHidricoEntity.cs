using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.User;

namespace back_end.src.Domain.CorpoHidrico
{
    public class CorpoHidricoEntity
    {
        public int Id { get; private set; }
        public string Nome { get; private set; }
        public string Localizacao { get; private set; }
        public double Tamanho { get; private set; }
        private bool EhPrivado { get; set; }

        public List<UserEntity> users { get; private set; }
        public List<Coleta.ColetaEntity> Coletas { get; private set; }
        public List<Imagem.ImagemEntity> Imagens { get; private set; }

        public CorpoHidricoEntity() { }

        public CorpoHidricoEntity(string nome, string localizacao, double tamanho, bool ehPrivado)
        {
            if (string.IsNullOrEmpty(nome))
                throw new ArgumentException("O nome do corpo hídrico é obrigatório.");

            if (string.IsNullOrEmpty(localizacao))
                throw new ArgumentException("A localização do corpo hídrico é obrigatória.");

            if (tamanho <= 0)
                throw new ArgumentException("O tamanho do corpo hídrico deve ser maior que zero.");

            Nome = nome;
            Localizacao = localizacao;
            Tamanho = tamanho;
            EhPrivado = ehPrivado;
        }
    }
}
