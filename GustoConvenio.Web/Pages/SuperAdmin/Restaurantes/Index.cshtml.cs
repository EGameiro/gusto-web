using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GustoConvenio.Web.Data.Repositories;
using GustoConvenio.Web.Models;

namespace GustoConvenio.Web.Pages.SuperAdmin.Restaurantes;

[Authorize(Policy = "SuperAdmin")]
public class IndexModel(IRestauranteRepository repo) : PageModel
{
    public List<Restaurante> Restaurantes { get; set; } = [];

    public async Task OnGetAsync()
    {
        Restaurantes = await repo.ListarTodosAsync();
    }
}
