using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace back_end.src.Medicoes.Application
{
    public interface IMedicao
    {

    }
    
    public sealed record InputMedicao(
        string codigoMedicao,
        double? valor,
        string unidade,
        bool ehCensurado = false,
        double? limite = null
    );

}