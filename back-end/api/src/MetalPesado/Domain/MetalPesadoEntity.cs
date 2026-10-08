using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back_end.src.Domain.Coleta;

namespace back_end.src.Domain.MetalPesado
{
    public class MetalPesadoEntity
    {
        public int Id { get; private set; }
        public string Nome { get; private set; }
        public double Concentracao { get; private set; }
        public string Unidade { get; private set; }
        [System.Text.Json.Serialization.JsonIgnore]
        public Coleta.ColetaEntity Coleta { get; private set; }

        public MetalPesadoEntity() { }

        public MetalPesadoEntity(string nome, double concentracao, string unidade, ColetaEntity coleta)
        {
            if (string.IsNullOrEmpty(nome))
                throw new ArgumentException("O nome do metal pesado é obrigatório.");

            if (!double.IsFinite(concentracao) || concentracao < 0)
                throw new ArgumentException(
                    "A concentração do metal pesado deve ser maior ou igual a zero."
                );

            if (string.IsNullOrEmpty(unidade))
                throw new ArgumentException("A unidade de medida do metal pesado é obrigatória.");

            Nome = nome;
            Concentracao = concentracao;
            Unidade = unidade;
            Coleta = coleta;
        }
    }
}
