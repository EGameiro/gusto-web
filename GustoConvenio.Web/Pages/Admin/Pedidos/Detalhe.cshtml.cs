using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GustoConvenio.Web.Data.Repositories;
using GustoConvenio.Web.Models;

namespace GustoConvenio.Web.Pages.Admin.Pedidos;

[Authorize(Policy = "Admin")]
public class DetalheModel(IPedidoRepository repo) : PageModel
{
    public string            NomeEmpresa { get; set; } = "";
    public PedidoResumo      Resumo      { get; set; } = null!;
    public List<ItemPedidoDto> Itens     { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var resultado = await repo.ObterDetalhesAdminAsync(id);
        if (resultado is null) return NotFound();

        NomeEmpresa = resultado.Value.NomeEmpresa;
        Resumo      = resultado.Value.resumo;
        Itens       = resultado.Value.itens;
        return Page();
    }

    public async Task<IActionResult> OnPostAtualizarStatusAsync(int id, string status)
    {
        var validos = new[] { "pendente", "preparo", "saiu", "entregue" };
        if (!validos.Contains(status)) return BadRequest();

        await repo.AtualizarStatusConvenioAsync(id, status);
        return RedirectToPage(new { id });
    }
}
