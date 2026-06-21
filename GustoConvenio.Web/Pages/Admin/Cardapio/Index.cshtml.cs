using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GustoConvenio.Web.Auth;
using GustoConvenio.Web.Data.Repositories;
using GustoConvenio.Web.Models;

namespace GustoConvenio.Web.Pages.Admin.Cardapio;

[Authorize(Policy = "Admin")]
public class IndexModel(ICardapioRepository repo, TenantService tenant) : PageModel
{
    public List<CardapioItem> Itens    { get; set; } = [];
    public int                DiaAtivo { get; set; }

    // Form de adição
    [BindProperty] public int    DiaSemana { get; set; }
    [BindProperty] public string Tipo      { get; set; } = "prato";
    [BindProperty, Required, MaxLength(200)]
    public string              Nome      { get; set; } = "";
    [BindProperty] public int  Ordem     { get; set; } = 0;

    private static readonly string[] Dias = ["Segunda","Terça","Quarta","Quinta","Sexta","Sábado"];
    public string[] NomesDias => Dias;

    public async Task OnGetAsync(int dia = 0)
    {
        DiaAtivo = Math.Clamp(dia, 0, 5);
        Itens    = (await repo.ListarTodosAsync(tenant.RestauranteId))
                    .Where(i => i.DiaSemana == DiaAtivo)
                    .ToList();
        DiaSemana = DiaAtivo;
    }

    public async Task<IActionResult> OnPostAdicionarAsync()
    {
        if (!ModelState.IsValid)
        {
            DiaAtivo = DiaSemana;
            Itens    = (await repo.ListarTodosAsync(tenant.RestauranteId))
                        .Where(i => i.DiaSemana == DiaAtivo)
                        .ToList();
            return Page();
        }

        await repo.CriarAsync(new CardapioItem
        {
            DiaSemana    = DiaSemana,
            Tipo         = Tipo,
            Nome         = Nome.Trim(),
            Ativo        = true,
            Ordem        = Ordem,
            RestauranteId = tenant.RestauranteId,
        });

        TempData["Sucesso"] = $"\"{Nome}\" adicionado ao cardápio de {Dias[DiaSemana]}.";
        return RedirectToPage(new { dia = DiaSemana });
    }

    public async Task<IActionResult> OnPostExcluirAsync(int id, int dia)
    {
        await repo.ExcluirAsync(id, tenant.RestauranteId);
        TempData["Sucesso"] = "Item removido do cardápio.";
        return RedirectToPage(new { dia });
    }
}
