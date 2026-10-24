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
        public bool EhPrivado { get; private set; }

        public List<UserEntity> users { get; private set; }
        public List<Coleta.ColetaEntity> Coletas { get; private set; } = [];
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
            users = new List<UserEntity>();
            Coletas = new List<Coleta.ColetaEntity>();
            Imagens = new List<Imagem.ImagemEntity>();
        }

        public void AtualizarDados(string nome, string localizacao, double tamanho, bool ehPrivado)
        {
            if (string.IsNullOrWhiteSpace(nome) || string.IsNullOrWhiteSpace(localizacao) || tamanho <= 0)
                throw new ArgumentException("Informe nome, localização e tamanho válidos.");
            Nome = nome;
            Localizacao = localizacao;
            Tamanho = tamanho;
            EhPrivado = ehPrivado;
        }
    }
}
