using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GustoConvenio.Web.Auth;
using GustoConvenio.Web.Data.Repositories;
using GustoConvenio.Web.Models;

namespace GustoConvenio.Web.Pages.Admin.Empresas;

[Authorize(Policy = "Admin")]
public class IndexModel(IEmpresaRepository repo, TenantService tenant) : PageModel
{
    public List<Empresa> Empresas { get; set; } = [];

    public async Task OnGetAsync()
    {
        Empresas = await repo.ListarTodasAsync(tenant.RestauranteId);
    }
}
