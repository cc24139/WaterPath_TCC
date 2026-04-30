using back_end.src.Domain.CianoBacteria;
using MediatR;

namespace Application.Queries.CianoBacteria;

public class QueryObterCianoBacteriaPorId : IRequest<CianoBacteriaEntity?>
{
    public int Id { get; set; }
}

public class QueryObterTodasCianoBacterias : IRequest<List<CianoBacteriaEntity>> { }
