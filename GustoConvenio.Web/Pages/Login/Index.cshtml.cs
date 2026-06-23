using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using BCrypt.Net;
using GustoConvenio.Web.Data.Repositories;

namespace GustoConvenio.Web.Pages.Login;

public class IndexModel(IEmpresaRepository empresaRepo, IAdminUserRepository adminRepo, IRestauranteRepository restauranteRepo) : PageModel
{
    [BindProperty, Required, EmailAddress]
    public string Email { get; set; } = "";

    [BindProperty, Required]
    public string Senha { get; set; } = "";

    public string Erro { get; set; } = "";

    public IActionResult OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectAfterLogin();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        // 1. Tenta como Admin
        var admin = await adminRepo.ObterPorEmailAsync(Email);
        if (admin is not null && BCrypt.Net.BCrypt.Verify(Senha, admin.SenhaHash))
        {
            var restauranteAdmin = await restauranteRepo.ObterPorIdAsync(admin.RestauranteId);
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, admin.Id.ToString()),
                new(ClaimTypes.Name, admin.Email),
                new("IsAdmin", "true"),
                new("EmpresaNome", admin.Nome),
                new("RestauranteId", admin.RestauranteId.ToString()),
                new("RestauranteNome", restauranteAdmin?.Nome ?? "Restaurante"),
                new("IsSuperAdmin", admin.IsSuperAdmin ? "true" : "false"),
            };
            await SignInAsync(claims);
            return admin.IsSuperAdmin
                ? RedirectToPage("/SuperAdmin/Restaurantes/Index")
                : RedirectToPage("/Admin/Pedidos/Index");
        }

        // 2. Tenta como Empresa
        var empresa = await empresaRepo.ObterPorEmailAsync(Email);
        if (empresa is not null
            && !string.IsNullOrEmpty(empresa.SenhaHash)
            && BCrypt.Net.BCrypt.Verify(Senha, empresa.SenhaHash))
        {
            var restauranteEmpresa = await restauranteRepo.ObterPorIdAsync(empresa.RestauranteId);
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, empresa.Id.ToString()),
                new(ClaimTypes.Name, empresa.Email!),
                new("EmpresaId",   empresa.Id.ToString()),
                new("EmpresaNome", empresa.NomeEmpresa),
                new("RestauranteNome", restauranteEmpresa?.Nome ?? "Restaurante"),
            };
            await SignInAsync(claims);
            return RedirectToPage("/Pedido/Index");
        }

        Erro = "E-mail ou senha inválidos.";
        return Page();
    }

    private async Task SignInAsync(List<Claim> claims)
    {
        var identity  = new ClaimsIdentity(claims, "GustoCookie");
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync("GustoCookie", principal,
            new AuthenticationProperties { IsPersistent = false });
    }

    private IActionResult RedirectAfterLogin()
    {
        if (User.FindFirst("IsAdmin")?.Value == "true")
            return RedirectToPage("/Admin/Pedidos/Index");
        return RedirectToPage("/Pedido/Index");
    }
}
