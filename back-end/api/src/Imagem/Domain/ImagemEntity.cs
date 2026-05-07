using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using back_end.src.Domain.Coleta;
using back_end.src.Domain.CorpoHidrico;

namespace back_end.src.Domain.Imagem
{
    public class ImagemEntity
    {
        private const string UrlBase = "src/resources/imagens/";
        public int Id { get; private set; }
        public string Url { get; private set; }
        public CorpoHidricoEntity CorpoHidrico { get; private set; }
        public ColetaEntity Coleta { get; private set; }
        public DateTime DataUpload { get; private set; }

        public ImagemEntity() { }

        public ImagemEntity(string url, CorpoHidricoEntity corpoHidrico)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException("A URL da imagem é obrigatória.");

            Url = $"{UrlBase}{url}";
            this.CorpoHidrico = corpoHidrico;
        }
    }
}