using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back_end.src.Domain.MetalPesado
{
    public class MetalPesadoEntity
    {
        public int Id { get; private set; }
        public string Nome { get; private set; }
        public float Concentracao { get; private set; }
        public string Unidade { get; private set; }
        public Coleta.ColetaEntity Coleta { get; private set; }

        public MetalPesadoEntity() { }

        public MetalPesadoEntity(string nome, float concentracao, string unidade)
        {
            if (string.IsNullOrEmpty(nome))
                throw new ArgumentException("O nome do metal pesado é obrigatório.");

            if (concentracao < 0)
                throw new ArgumentException(
                    "A concentração do metal pesado deve ser maior ou igual a zero."
                );

            if (string.IsNullOrEmpty(unidade))
                throw new ArgumentException("A unidade de medida do metal pesado é obrigatória.");

            Nome = nome;
            Concentracao = concentracao;
            Unidade = unidade;
        }
    }
}
