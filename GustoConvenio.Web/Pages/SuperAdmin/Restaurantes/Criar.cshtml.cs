using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GustoConvenio.Web.Data.Repositories;
using GustoConvenio.Web.Models;

namespace GustoConvenio.Web.Pages.SuperAdmin.Restaurantes;

[Authorize(Policy = "SuperAdmin")]
public class CriarModel(IRestauranteRepository repoR, IAdminUserRepository repoA) : PageModel
{
    [BindProperty, Required, MaxLength(200)] public string Nome      { get; set; } = "";
    [BindProperty, Required, MaxLength(100)] public string Slug      { get; set; } = "";
    [BindProperty, MaxLength(20)]            public string? Telefone { get; set; }
    [BindProperty, EmailAddress, MaxLength(150)] public string? Email { get; set; }
    [BindProperty, MaxLength(300)]           public string? Endereco { get; set; }

    [BindProperty, Required, MaxLength(200)] public string AdminNome  { get; set; } = "";
    [BindProperty, Required, EmailAddress]   public string AdminEmail { get; set; } = "";
    [BindProperty, Required, MinLength(6)]   public string AdminSenha { get; set; } = "";

    public IActionResult OnGet() => Page();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var restaurante = new Restaurante
        {
            Nome     = Nome.Trim(),
            Slug     = Slug.Trim().ToLower(),
            Telefone = Telefone?.Trim(),
            Email    = Email?.Trim().ToLower(),
            Endereco = Endereco?.Trim(),
            Ativo    = true,
        };

        var restauranteId = await repoR.CriarAsync(restaurante);

        await repoA.CriarAsync(new AdminUser
        {
            Nome          = AdminNome.Trim(),
            Email         = AdminEmail.Trim().ToLower(),
            SenhaHash     = BCrypt.Net.BCrypt.HashPassword(AdminSenha),
            Ativo         = true,
            RestauranteId = restauranteId,
            IsSuperAdmin  = false,
        });

        TempData["Sucesso"] = $"Restaurante \"{Nome}\" criado com sucesso.";
        return RedirectToPage("/SuperAdmin/Restaurantes/Index");
    }
}
