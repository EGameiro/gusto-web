using GustoConvenio.Web.Models;

namespace GustoConvenio.Web.Data.Repositories;

public interface IAdminUserRepository
{
    Task<AdminUser?> ObterPorEmailAsync(string email);
    Task<int> CriarAsync(AdminUser user);
}
