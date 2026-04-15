using back_end.src.Domain.Coleta;
using MediatR;

namespace Application.Queries.Coleta;

public class QueryObterColetaPorId : IRequest<ColetaEntity?>
{
    public int Id { get; set; }
}

public class QueryObterTodasColetas : IRequest<List<ColetaEntity>> { }
public class QueryObterColetasPeriodo : IRequest<List<ColetaEntity>>
{
    public int CorpoHidricoId { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
}
public class QueryObterColetaRecente : IRequest<ColetaEntity?>
{
    public int CorpoHidricoId { get; set; }
}
