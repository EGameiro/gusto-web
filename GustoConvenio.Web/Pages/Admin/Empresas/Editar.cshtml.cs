using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GustoConvenio.Web.Data.Repositories;

namespace GustoConvenio.Web.Pages.Admin.Empresas;

[Authorize(Policy = "Admin")]
public class EditarModel(IEmpresaRepository repo) : PageModel
{
    [BindProperty] public int Id { get; set; }

    [BindProperty, Required, MaxLength(200)]
    public string NomeEmpresa { get; set; } = "";

    [BindProperty, MaxLength(20)]
    public string? NumeroWhatsapp { get; set; }

    [BindProperty, EmailAddress, MaxLength(150)]
    public string? Email { get; set; }

    [BindProperty]
    public string? NovaSenha { get; set; }

    [BindProperty]
    public string HorarioCorte { get; set; } = "10:00";

    [BindProperty]
    public bool Ativo { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var e = await repo.ObterPorIdAsync(id);
        if (e is null) return NotFound();

        Id             = e.Id;
        NomeEmpresa    = e.NomeEmpresa;
        NumeroWhatsapp = e.NumeroWhatsapp;
        Email          = e.Email;
        HorarioCorte   = e.HorarioCorte.ToString("HH:mm");
        Ativo          = e.Ativo;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        if (!TimeOnly.TryParse(HorarioCorte, out var corte))
        {
            ModelState.AddModelError("HorarioCorte", "Horário inválido.");
            return Page();
        }

        var e = await repo.ObterPorIdAsync(Id);
        if (e is null) return NotFound();

        e.NomeEmpresa    = NomeEmpresa.Trim();
        e.NumeroWhatsapp = NumeroWhatsapp?.Trim();
        e.Email          = Email?.Trim().ToLower();
        e.HorarioCorteRaw = corte.ToTimeSpan();
        e.Ativo          = Ativo;

        if (!string.IsNullOrWhiteSpace(NovaSenha))
        {
            if (NovaSenha.Length < 6)
            {
                ModelState.AddModelError("NovaSenha", "Senha deve ter ao menos 6 caracteres.");
                return Page();
            }
            e.SenhaHash = BCrypt.Net.BCrypt.HashPassword(NovaSenha)!;
        }

        await repo.AtualizarAsync(e);

        TempData["Sucesso"] = "Empresa atualizada com sucesso.";
        return RedirectToPage("/Admin/Empresas/Index");
    }
}
