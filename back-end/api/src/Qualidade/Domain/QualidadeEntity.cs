using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back_end.src.Domain.CorpoHidrico;
using back_end.src.Domain.MetalPesado;

namespace back_end.src.Domain.Qualidade
{
    public class QualidadeEntity
    {
        public int Id { get; private set; }
        public CorpoHidricoEntity CorpoHidrico { get; private set; }
        public int IQA { get; private set; }
        public List<MetalPesadoEntity> MetaisPesados { get; private set; }
        public List<CianoBacteria.CianoBacteriaEntity> CianoBacterias { get; private set; }
        public QualidadeFutura.QualidadeFuturaEntity QualidadeFutura { get; private set; }

        public QualidadeEntity() { }

        public QualidadeEntity(
            CorpoHidricoEntity corpoHidrico,
            int iqa,
            QualidadeFutura.QualidadeFuturaEntity qualidadeFutura
        )
        {
            CorpoHidrico = corpoHidrico;
            IQA = iqa;
            MetaisPesados = new List<MetalPesadoEntity>();
            CianoBacterias = new List<CianoBacteria.CianoBacteriaEntity>();
            QualidadeFutura = qualidadeFutura;
        }
    }
}
