using GustoConvenio.Web.Models;

namespace GustoConvenio.Web.Data.Repositories;

public interface IFuncionarioRepository
{
    Task<List<Funcionario>> ListarAsync(int empresaId);
    Task<Funcionario?> ObterPorIdAsync(int id, int empresaId);
    Task<int> CriarAsync(Funcionario funcionario);
    Task AtualizarAsync(Funcionario funcionario);
    Task DesativarAsync(int id, int empresaId);
}
