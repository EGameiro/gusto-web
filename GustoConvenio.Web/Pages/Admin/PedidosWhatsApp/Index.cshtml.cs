using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GustoConvenio.Web.Auth;
using GustoConvenio.Web.Data.Repositories;
using GustoConvenio.Web.Models;

namespace GustoConvenio.Web.Pages.Admin.PedidosWhatsApp;

[Authorize(Policy = "Admin")]
public class IndexModel(IPedidoRepository repo, TenantService tenant) : PageModel
{
    public List<PedidoWhatsApp> Pedidos    { get; set; } = [];
    public int  EmPreparo  { get; set; }
    public int  Saiu       { get; set; }
    public int  Entregues  { get; set; }
    public string? StatusAtivo { get; set; }

    private static readonly string[] StatusValidos = ["preparo", "saiu", "entregue"];

    public async Task OnGetAsync(string? status = null)
    {
        StatusAtivo = StatusValidos.Contains(status) ? status : null;
        Pedidos     = await repo.ListarWhatsAppHojeAsync(tenant.RestauranteId, StatusAtivo);
        (EmPreparo, Saiu, Entregues) = await repo.TotaisWhatsAppHojeAsync(tenant.RestauranteId);
    }

    public async Task<IActionResult> OnPostAvancarStatusAsync(int pedidoId, string statusAtual, string? filtro)
    {
        var novoStatus = statusAtual switch
        {
            "preparo" => "saiu",
            "saiu"    => "entregue",
            _         => null
        };

        if (novoStatus is not null)
            await repo.AtualizarStatusWhatsAppAsync(pedidoId, novoStatus);

        return RedirectToPage(new { status = filtro });
    }
}
