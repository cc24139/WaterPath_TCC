using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back_end.src.Domain.CianoBacteria;
using back_end.src.Domain.CorpoHidrico;
using back_end.src.Domain.Imagem;
using back_end.src.Domain.MetalPesado;
using back_end.src.Medicoes.Domain;

namespace back_end.src.Domain.Coleta
{
    public class ColetaEntity
    {
        public int Id { get;  set; }
        public int CorpoHidricoId { get;  set; }
        public CorpoHidricoEntity CorpoHidrico { get; private set; } = null!;
        public List<ImagemEntity> Imagens { get; private set; } = [];
        public List<MetalPesadoEntity> MetaisPesados { get; private set; } = [];
        public List<CianoBacteriaEntity> CianoBacterias { get; private set; } = [];
        public DateTimeOffset DataHora { get;  set; }
        public double? Latitude { get;  set; }
        public double? Longitude { get;  set; }
        public double? ProfundidadeMetros { get;  set; }


        public ColetaEntity() { }
        public ColetaEntity(
            int corpoHidricoId,
            DateTimeOffset dataHora,
            double? latitude,
            double? Longitude,
            double? profundidade
        )
        {
            if (corpoHidricoId <= 0)
                throw new ArgumentException("Id inválido!");

            this.CorpoHidricoId = corpoHidricoId;
            this.DataHora = dataHora;
            this.Latitude = latitude;
            this.Longitude = Longitude;
            this.ProfundidadeMetros = profundidade;
        }

        public List<MedicoesEntity> Medicoes { get; private set; } = [];

        public void AdicionarMedicao(MedicoesEntity medicao)
        {
            ArgumentNullException.ThrowIfNull(medicao);
            if (medicao.ColetaId != 0 && medicao.ColetaId != Id)
                throw new InvalidOperationException("A medição pertence a outra coleta.");
            if (Id > 0)
                medicao.VincularColeta(Id);

            var existente = Medicoes.Any(x => x.codigoMedicao == medicao.codigoMedicao);

            if (existente)
                throw new InvalidOperationException(
                    $"Já existe uma medição de {medicao.codigoMedicao}.");

            Medicoes.Add(medicao);
        }
    }
}
