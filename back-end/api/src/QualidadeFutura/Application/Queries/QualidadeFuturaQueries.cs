using back_end.src.Domain.QualidadeFutura;
using back_end.src.Medicoes.Domain;
using MediatR;

namespace Application.Queries.QualidadeFutura;

public class QueryObterQualidadeFuturaPorId : IRequest<QualidadeFuturaEntity?>
{
    public int Id { get; set; }
}

public class QueryObterTodasQualidadesFuturas : IRequest<List<QualidadeFuturaEntity>> { }

public class QueryObterMedicaoDaQualidadeFutura : IRequest<List<MedicoesEntity>>
{
    public int Id { get; set; }
    public Medicao CodigoMedicao { get; set; }
}
