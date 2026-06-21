using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GustoConvenio.Web.Auth;
using GustoConvenio.Web.Data.Repositories;
using GustoConvenio.Web.Models;

namespace GustoConvenio.Web.Pages.Funcionarios;

[Authorize(Policy = "Empresa")]
public class CriarModel(IFuncionarioRepository repo, TenantService tenant) : PageModel
{
    [BindProperty, Required(ErrorMessage = "Nome obrigatório"), MaxLength(200)]
    public string Nome { get; set; } = "";

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        await repo.CriarAsync(new Funcionario
        {
            EmpresaId = tenant.EmpresaId,
            Nome      = Nome.Trim(),
            Ativo     = true,
        });

        TempData["Sucesso"] = "Funcionário cadastrado com sucesso.";
        return RedirectToPage("/Funcionarios/Index");
    }
}
