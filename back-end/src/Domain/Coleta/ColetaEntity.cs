using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back_end.src.Domain.CianoBacteria;
using back_end.src.Domain.CorpoHidrico;
using back_end.src.Domain.Imagem;
using back_end.src.Domain.MetalPesado;

namespace back_end.src.Domain.Coleta
{
    public class ColetaEntity
    {
        public int Id { get; private set; }
        public string Nome { get; private set; }
        public DateTime Data { get; private set; }
        public float Ph { get; private set; }
        public float OxigenioDissolvido { get; private set; }
        public float Turbidez { get; private set; }
        public float CloroResidual { get; private set; }
        public float Floretos { get; private set; }
        public float ColiformesTotais { get; private set; }
        public bool EscherichiaColi { get; private set; }

        public CorpoHidricoEntity CorpoHidrico { get; set; }
        public List<ImagemEntity> Imagens { get; private set; }
        public List<MetalPesadoEntity> MetaisPesados { get; private set; }
        public List<CianoBacteriaEntity> CianoBacterias { get; private set; }

        public ColetaEntity() { }

        public ColetaEntity(
            string nome,
            DateTime data,
            float ph,
            float oxigenioDissolvido,
            float turbidez,
            float cloroResidual,
            float floretos,
            float coliformesTotais,
            bool escherichiaColi,
            CorpoHidricoEntity corpoHidrico
        )
        {
            if (string.IsNullOrEmpty(nome))
                throw new ArgumentException("O nome da coleta é obrigatório.");

            if (data == default)
                throw new ArgumentException("A data da coleta é obrigatória.");

            if (ph < 0 || ph > 14)
                throw new ArgumentException("O pH deve estar entre 0 e 14.");

            if (oxigenioDissolvido < 0)
                throw new ArgumentException(
                    "A concentração de oxigênio dissolvido deve ser maior ou igual a zero."
                );

            if (turbidez < 0)
                throw new ArgumentException("A turbidez deve ser maior ou igual a zero.");

            if (cloroResidual < 0)
                throw new ArgumentException(
                    "A concentração de cloro residual deve ser maior ou igual a zero."
                );

            if (floretos < 0)
                throw new ArgumentException(
                    "A concentração de floretos deve ser maior ou igual a zero."
                );

            if (coliformesTotais < 0)
                throw new ArgumentException(
                    "A concentração de coliformes totais deve ser maior ou igual a zero."
                );

            Nome = nome;
            Data = data;
            Ph = ph;
            OxigenioDissolvido = oxigenioDissolvido;
            Turbidez = turbidez;
            CloroResidual = cloroResidual;
            Floretos = floretos;
            ColiformesTotais = coliformesTotais;
            EscherichiaColi = escherichiaColi;
            CorpoHidrico = corpoHidrico;
            Imagens = new List<ImagemEntity>();
            MetaisPesados = new List<MetalPesadoEntity>();
            CianoBacterias = new List<CianoBacteriaEntity>();
        }
    }
}
