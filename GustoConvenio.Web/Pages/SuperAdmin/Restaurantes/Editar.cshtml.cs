using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GustoConvenio.Web.Data.Repositories;

namespace GustoConvenio.Web.Pages.SuperAdmin.Restaurantes;

[Authorize(Policy = "SuperAdmin")]
public class EditarModel(IRestauranteRepository repo) : PageModel
{
    [BindProperty] public int Id { get; set; }
    [BindProperty, Required, MaxLength(200)] public string Nome      { get; set; } = "";
    [BindProperty, Required, MaxLength(100)] public string Slug      { get; set; } = "";
    [BindProperty, MaxLength(20)]            public string? Telefone { get; set; }
    [BindProperty, EmailAddress, MaxLength(150)] public string? Email { get; set; }
    [BindProperty, MaxLength(300)]           public string? Endereco { get; set; }
    [BindProperty]                           public bool Ativo       { get; set; } = true;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var r = await repo.ObterPorIdAsync(id);
        if (r is null) return NotFound();

        Id       = r.Id;
        Nome     = r.Nome;
        Slug     = r.Slug;
        Telefone = r.Telefone;
        Email    = r.Email;
        Endereco = r.Endereco;
        Ativo    = r.Ativo;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var r = await repo.ObterPorIdAsync(Id);
        if (r is null) return NotFound();

        r.Nome     = Nome.Trim();
        r.Slug     = Slug.Trim().ToLower();
        r.Telefone = Telefone?.Trim();
        r.Email    = Email?.Trim().ToLower();
        r.Endereco = Endereco?.Trim();
        r.Ativo    = Ativo;

        await repo.AtualizarAsync(r);

        TempData["Sucesso"] = "Restaurante atualizado com sucesso.";
        return RedirectToPage("/SuperAdmin/Restaurantes/Index");
    }
}
