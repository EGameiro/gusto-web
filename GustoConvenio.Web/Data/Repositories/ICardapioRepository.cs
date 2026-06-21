using GustoConvenio.Web.Models;

namespace GustoConvenio.Web.Data.Repositories;

public interface ICardapioRepository
{
    Task<List<CardapioItem>> ListarPorDiaAsync(int diaSemana, int restauranteId);
    Task<List<CardapioItem>> ListarTodosAsync(int restauranteId);
    Task<int> CriarAsync(CardapioItem item);
    Task ExcluirAsync(int id, int restauranteId);
}
