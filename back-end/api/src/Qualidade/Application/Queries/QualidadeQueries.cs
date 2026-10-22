using back_end.src.Domain.Qualidade;
using back_end.src.Medicoes.Domain;
using MediatR;

namespace Application.Queries.Qualidade;

public class QueryObterQualidadePorId : IRequest<QualidadeEntity?>
{
    public int Id { get; set; }
}

public class QueryObterTodasQualidades : IRequest<List<QualidadeEntity>> { }

public class QueryObterMedicaoDaQualidade : IRequest<List<MedicoesEntity>>
{
    public int Id { get; set; }
    public Medicao CodigoMedicao { get; set; }
}
