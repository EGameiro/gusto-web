using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GustoConvenio.Web.Auth;
using GustoConvenio.Web.Data.Repositories;
using GustoConvenio.Web.Models;

namespace GustoConvenio.Web.Pages.Funcionarios;

[Authorize(Policy = "Empresa")]
public class IndexModel(IFuncionarioRepository repo, TenantService tenant) : PageModel
{
    public List<Funcionario> Funcionarios { get; set; } = [];

    public async Task OnGetAsync()
    {
        Funcionarios = await repo.ListarAsync(tenant.EmpresaId);
    }
}
