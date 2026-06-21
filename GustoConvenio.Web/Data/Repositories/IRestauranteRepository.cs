using GustoConvenio.Web.Models;

namespace GustoConvenio.Web.Data.Repositories;

public interface IRestauranteRepository
{
    Task<List<Restaurante>> ListarTodosAsync();
    Task<Restaurante?> ObterPorIdAsync(int id);
    Task<int> CriarAsync(Restaurante r);
    Task AtualizarAsync(Restaurante r);
}
