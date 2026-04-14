using back_end.src.Domain.Coleta;
using MediatR;

namespace Application.Queries.Coleta;

public class QueryObterColetaPorId : IRequest<ColetaEntity?>
{
    public int Id { get; set; }
}

public class QueryObterTodasColetas : IRequest<List<ColetaEntity>> { }
