using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GustoConvenio.Web.Auth;
using GustoConvenio.Web.Data.Repositories;
using GustoConvenio.Web.Models;

namespace GustoConvenio.Web.Pages.Admin.Cardapio;

[Authorize(Policy = "Admin")]
public class IndexModel(ICardapioRepository repo, IEmpresaRepository empresaRepo, TenantService tenant) : PageModel
{
    public List<CardapioItem> Itens       { get; set; } = [];
    public List<Empresa>      Empresas    { get; set; } = [];
    public int                DiaAtivo    { get; set; }
    public int?               EmpresaAtiva { get; set; }
    public bool               WhatsAppAtivo { get; set; }

    [BindProperty] public int      DiaSemana  { get; set; }
    [BindProperty] public string   Tipo       { get; set; } = "prato";
    [BindProperty, Required, MaxLength(200)]
    public string               Nome       { get; set; } = "";
    [BindProperty] public int      Ordem      { get; set; } = 0;
    [BindProperty] public int?     EmpresaId  { get; set; }
    [BindProperty] public bool     IsWhatsApp { get; set; }
    [BindProperty] public decimal? Preco          { get; set; }
    [BindProperty] public decimal? PrecoMini      { get; set; }
    [BindProperty] public decimal? PrecoNormal    { get; set; }
    [BindProperty] public decimal? PrecoExecutiva { get; set; }

    private static readonly string[] Dias = ["Segunda","Terça","Quarta","Quinta","Sexta","Sábado"];
    public string[] NomesDias => Dias;

    public async Task OnGetAsync(int dia = 0, string? empresaId = null)
    {
        DiaAtivo      = Math.Clamp(dia, 0, 5);
        DiaSemana     = DiaAtivo;
        WhatsAppAtivo = empresaId == "whatsapp";
        EmpresaAtiva  = int.TryParse(empresaId, out var eid) ? eid : null;
        EmpresaId     = EmpresaAtiva;
        IsWhatsApp    = WhatsAppAtivo;

        Empresas = await empresaRepo.ListarTodasAsync(tenant.RestauranteId);

        // WhatsApp → empresa_id IS NULL; conveniada → empresa_id = X
        var filtroEmpresa = WhatsAppAtivo ? (int?)null : EmpresaAtiva;
        var deveCarregar  = WhatsAppAtivo || EmpresaAtiva.HasValue;
        Itens = deveCarregar
            ? await repo.ListarPorDiaEEmpresaAsync(DiaAtivo, tenant.RestauranteId, filtroEmpresa)
            : [];
    }

    public async Task<IActionResult> OnPostAdicionarAsync()
    {
        var filtroEmpresa = IsWhatsApp ? (int?)null : EmpresaId;

        if (!ModelState.IsValid)
        {
            DiaAtivo      = DiaSemana;
            EmpresaAtiva  = EmpresaId;
            WhatsAppAtivo = IsWhatsApp;
            Empresas      = await empresaRepo.ListarTodasAsync(tenant.RestauranteId);
            Itens         = await repo.ListarPorDiaEEmpresaAsync(DiaAtivo, tenant.RestauranteId, filtroEmpresa);
            return Page();
        }

        await repo.CriarAsync(new CardapioItem
        {
            DiaSemana     = DiaSemana,
            Tipo          = Tipo,
            Nome          = Nome.Trim(),
            Ativo         = true,
            Ordem         = Ordem,
            RestauranteId = tenant.RestauranteId,
            EmpresaId     = filtroEmpresa,
            Preco         = Preco,
            PrecoMini      = Tipo == "prato" ? PrecoMini      : null,
            PrecoNormal    = Tipo == "prato" ? PrecoNormal    : null,
            PrecoExecutiva = Tipo == "prato" ? PrecoExecutiva : null,
        });

        TempData["Sucesso"] = $"\"{Nome}\" adicionado ao cardápio de {Dias[DiaSemana]}.";
        var redirect = IsWhatsApp ? "whatsapp" : EmpresaId?.ToString();
        return RedirectToPage(new { dia = DiaSemana, empresaId = redirect });
    }

    public async Task<IActionResult> OnPostExcluirAsync(int id, int dia, string? empresaId)
    {
        await repo.ExcluirAsync(id, tenant.RestauranteId);
        TempData["Sucesso"] = "Item removido do cardápio.";
        return RedirectToPage(new { dia, empresaId });
    }
}
