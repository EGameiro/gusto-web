using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GustoConvenio.Web.Auth;
using GustoConvenio.Web.Data.Repositories;
using GustoConvenio.Web.Models;

namespace GustoConvenio.Web.Pages.Historico;

[Authorize(Policy = "Empresa")]
public class DetalhesModel(IPedidoRepository repo, TenantService tenant) : PageModel
{
    public PedidoResumo      Resumo { get; set; } = null!;
    public List<ItemPedidoDto> Itens  { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var resultado = await repo.ObterDetalhesAsync(id, tenant.EmpresaId);
        if (resultado is null) return NotFound();

        Resumo = resultado.Value.resumo;
        Itens  = resultado.Value.itens;
        return Page();
    }
}
