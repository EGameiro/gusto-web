using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BCrypt.Net;
using GustoConvenio.Web.Auth;
using GustoConvenio.Web.Data.Repositories;
using GustoConvenio.Web.Models;

namespace GustoConvenio.Web.Pages.Admin.Empresas;

[Authorize(Policy = "Admin")]
public class CriarModel(IEmpresaRepository repo, TenantService tenant) : PageModel
{
    [BindProperty, Required, MaxLength(200)]
    public string NomeEmpresa { get; set; } = "";

    [BindProperty, MaxLength(20)]
    public string? NumeroWhatsapp { get; set; }

    [BindProperty, EmailAddress, MaxLength(150)]
    public string? Email { get; set; }

    [BindProperty]
    public string Senha { get; set; } = "";

    [BindProperty]
    public string HorarioCorte { get; set; } = "10:00";

    [BindProperty]
    public bool Ativo { get; set; } = true;

    public IActionResult OnGet() => Page();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        if (string.IsNullOrWhiteSpace(Senha) || Senha.Length < 6)
        {
            ModelState.AddModelError("Senha", "Senha deve ter ao menos 6 caracteres.");
            return Page();
        }

        if (!TimeOnly.TryParse(HorarioCorte, out var corte))
        {
            ModelState.AddModelError("HorarioCorte", "Horário inválido.");
            return Page();
        }

        var empresa = new Empresa
        {
            NomeEmpresa    = NomeEmpresa.Trim(),
            NumeroWhatsapp = NumeroWhatsapp?.Trim(),
            Email          = Email?.Trim().ToLower(),
            SenhaHash      = BCrypt.Net.BCrypt.HashPassword(Senha)!,
            HorarioCorteRaw = corte.ToTimeSpan(),
            Ativo          = Ativo,
            RestauranteId  = tenant.RestauranteId,
        };

        await repo.CriarAsync(empresa);

        TempData["Sucesso"] = $"Empresa \"{empresa.NomeEmpresa}\" criada com sucesso.";
        return RedirectToPage("/Admin/Empresas/Index");
    }
}
