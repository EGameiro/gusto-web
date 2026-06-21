using GustoConvenio.Web.Models;

namespace GustoConvenio.Web.Data.Repositories;

public interface IEmpresaRepository
{
    Task<Empresa?> ObterPorEmailAsync(string email);
    Task<Empresa?> ObterPorIdAsync(int id);
    Task<List<Empresa>> ListarTodasAsync(int restauranteId);
    Task<int> CriarAsync(Empresa empresa);
    Task AtualizarAsync(Empresa empresa);
}
