using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back_end.src.Domain.CianoBacteria;
using back_end.src.Domain.CorpoHidrico;
using back_end.src.Domain.Imagem;
using back_end.src.Domain.MetalPesado;
using back_end.src.Domain.Qualidade;

namespace back_end.src.Domain.QualidadeFutura
{
    public class QualidadeFuturaEntity
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

        public CorpoHidricoEntity CorpoHidrico { get; private set; }
        public List<ImagemEntity> Imagens { get; private set; }
        public List<MetalPesadoEntity> MetaisPesados { get; private set; }
        public List<CianoBacteriaEntity> CianoBacterias { get; private set; }
        public List<QualidadeEntity> Qualidade { get; private set; }

        public QualidadeFuturaEntity() { }

        public QualidadeFuturaEntity(
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
            Qualidade = new List<QualidadeEntity>();
        }
    }
}
