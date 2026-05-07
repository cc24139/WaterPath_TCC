using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back_end.src.Domain.Coleta;

namespace back_end.src.Domain.CianoBacteria
{
    public class CianoBacteriaEntity
    {
        public int Id { get; private set; }
        public string Tipo { get; private set; }
        public float Concentracao { get; private set; }
        public string Unidade { get; private set; }
        public ColetaEntity Coleta { get; private set; }

        public CianoBacteriaEntity() { }

        public CianoBacteriaEntity(string tipo, float concentracao, string unidade, ColetaEntity coleta)
        {
            if (string.IsNullOrEmpty(tipo))
                throw new ArgumentException("O tipo de cianobactéria é obrigatório.");

            if (concentracao < 0)
                throw new ArgumentException(
                    "A concentração de cianobactéria deve ser maior ou igual a zero."
                );

            if (string.IsNullOrEmpty(unidade))
                throw new ArgumentException("A unidade de medida da cianobactéria é obrigatória.");

            Tipo = tipo;
            Concentracao = concentracao;
            Unidade = unidade;
            Coleta = coleta;
        }
    }
}
