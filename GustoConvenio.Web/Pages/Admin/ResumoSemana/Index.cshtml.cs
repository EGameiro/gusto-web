using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GustoConvenio.Web.Data.Repositories;

namespace GustoConvenio.Web.Pages.Admin.ResumoSemana;

[Authorize(Policy = "Admin")]
public class IndexModel(IPedidoRepository repo) : PageModel
{
    public List<(DateTime Dia, decimal TotalConvenio, decimal TotalWhatsApp)> Dias { get; set; } = [];
    public decimal TotalGeral       => Dias.Sum(d => d.TotalConvenio + d.TotalWhatsApp);
    public decimal TotalConvenio    => Dias.Sum(d => d.TotalConvenio);
    public decimal TotalWhatsApp    => Dias.Sum(d => d.TotalWhatsApp);

    public async Task OnGetAsync()
    {
        Dias = await repo.ResumoSemanaAsync();
    }
}
