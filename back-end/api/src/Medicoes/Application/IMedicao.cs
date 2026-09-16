using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back_end.src.Medicoes.Application
{

    
    public  record InputMedicao(
        string codigoMedicao,
        double? valor,
        string unidade,
        bool ehCensurado = false,
        double? limite = null
    );

    public  record InputHistorico(

    DateTimeOffset datafinal,

    IReadOnlyCollection<InputMedicao> medicoes);

    public  record PredictionInput(
        int idCorpoHidrico,
        DateTimeOffset dataFinal,
        double? DepthM,
        IReadOnlyCollection<InputMedicao> medicoesAtuais,
        IReadOnlyCollection<InputHistorico> historico);

    public  record MetalPrediction(
        string nome,
        double valor,
        string unidade,
        double? diminuicao,
        double? aumento,
        double? limite
        );

    public  record PredictionResult(

        string ModelVersion,
        IReadOnlyCollection<MetalPrediction> Predictions,
        IReadOnlyCollection<string> MissingFeatures,
        IReadOnlyCollection<string> ImputedFeatures,
        IReadOnlyCollection<string> Warnings);


}