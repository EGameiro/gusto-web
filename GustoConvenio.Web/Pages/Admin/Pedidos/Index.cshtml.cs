using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GustoConvenio.Web.Data.Repositories;

namespace GustoConvenio.Web.Pages.Admin.Pedidos;

[Authorize(Policy = "Admin")]
public class IndexModel(IPedidoRepository repo) : PageModel
{
    public List<(string NomeEmpresa, int TotalMarmitas, string Status, int PedidoId)> Pedidos { get; set; } = [];
    public int TotalEmpresas  { get; set; }
    public int TotalMarmitas  { get; set; }
    public string DataHoje    { get; set; } = "";

    public async Task OnGetAsync()
    {
        DataHoje     = DateTime.Today.ToString("dddd, dd/MM/yyyy");
        Pedidos      = await repo.ListarTodosHojeAsync();
        TotalEmpresas = Pedidos.Count;
        TotalMarmitas = Pedidos.Sum(p => p.TotalMarmitas);
    }
}
