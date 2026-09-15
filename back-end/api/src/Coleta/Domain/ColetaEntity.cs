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
        public int Id { get; private set; }
        public int CorpoHidricoId { get; private set; }
        public DateTimeOffset DataHora { get; private set; }
        public double? Latitude { get; private set; }
        public double? Longitude { get; private set; }
        public double? ProfundidadeMetros { get; private set; }

        public ColetaEntity() { }
        public ColetaEntity(
            int corpoHidricoId,
            DateTimeOffset dataHora,
            double? latitude,
            double? Longitude,
            double? profundidade
        )
        {
            if (corpoHidricoId < 0)
                throw new Exception("Id inválido!");

            this.CorpoHidricoId = corpoHidricoId;
            this.DataHora = dataHora;
            this.Latitude = latitude;
            this.Longitude = Longitude;
            this.ProfundidadeMetros = profundidade;
        }

        public List<MedicoesEntity> Medicoes { get; private set; } = [];

        public void AdicionarMedicao(MedicoesEntity medicao)
        {
            var existente = Medicoes.Any(x => x.codigoMedicao == medicao.codigoMedicao);

            if (existente)
                throw new InvalidOperationException(
                    $"Já existe uma medição de {medicao.codigoMedicao}.");

            Medicoes.Add(medicao);
        }
    }
}
