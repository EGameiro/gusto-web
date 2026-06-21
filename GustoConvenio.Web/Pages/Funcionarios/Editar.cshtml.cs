using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GustoConvenio.Web.Auth;
using GustoConvenio.Web.Data.Repositories;
using GustoConvenio.Web.Models;

namespace GustoConvenio.Web.Pages.Funcionarios;

[Authorize(Policy = "Empresa")]
public class EditarModel(IFuncionarioRepository repo, TenantService tenant) : PageModel
{
    [BindProperty] public int Id { get; set; }

    [BindProperty, Required(ErrorMessage = "Nome obrigatório"), MaxLength(200)]
    public string Nome { get; set; } = "";

    [BindProperty] public bool Ativo { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var f = await repo.ObterPorIdAsync(id, tenant.EmpresaId);
        if (f is null) return NotFound();

        Id    = f.Id;
        Nome  = f.Nome;
        Ativo = f.Ativo;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var f = await repo.ObterPorIdAsync(Id, tenant.EmpresaId);
        if (f is null) return NotFound();

        f.Nome  = Nome.Trim();
        f.Ativo = Ativo;
        await repo.AtualizarAsync(f);

        TempData["Sucesso"] = "Funcionário atualizado com sucesso.";
        return RedirectToPage("/Funcionarios/Index");
    }
}
