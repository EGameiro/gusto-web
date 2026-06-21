using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GustoConvenio.Web.Auth;
using GustoConvenio.Web.Data.Repositories;

namespace GustoConvenio.Web.Pages.Historico;

[Authorize(Policy = "Empresa")]
public class IndexModel(IPedidoRepository repo, TenantService tenant) : PageModel
{
    public List<PedidoResumo> Pedidos       { get; set; } = [];
    public int                TotalPedidos  { get; set; }
    public int                TotalMarmitas { get; set; }
    public decimal            TotalValor    { get; set; }
    public int                Mes           { get; set; }
    public int                Ano           { get; set; }

    public async Task OnGetAsync(int? mes, int? ano)
    {
        Mes = mes ?? DateTime.Today.Month;
        Ano = ano ?? DateTime.Today.Year;

        Pedidos       = await repo.ListarPorEmpresaAsync(tenant.EmpresaId, Mes, Ano);
        TotalPedidos  = Pedidos.Count;
        TotalMarmitas = Pedidos.Sum(p => p.TotalMarmitas);
        TotalValor    = Pedidos.Sum(p => p.TotalValor);
    }
}
