using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GustoConvenio.Web.Auth;
using GustoConvenio.Web.Data.Repositories;
using GustoConvenio.Web.Models;

namespace GustoConvenio.Web.Pages.Pedido;

[Authorize(Policy = "Empresa")]
public class IndexModel(
    IFuncionarioRepository funcRepo,
    ICardapioRepository    cardapioRepo,
    IPedidoRepository      pedidoRepo,
    IEmpresaRepository     empresaRepo,
    TenantService          tenant) : PageModel
{
    public List<Funcionario>  Funcionarios    { get; set; } = [];
    public List<CardapioItem> Pratos          { get; set; } = [];
    public List<CardapioItem> Acompanhamentos { get; set; } = [];
    public TimeOnly           HorarioCorte    { get; set; }
    public bool               JaPediuHoje     { get; set; }
    public string             DataHoje        { get; set; } = "";

    [BindProperty]
    public string PedidoJson { get; set; } = "";

    public async Task OnGetAsync()
    {
        await CarregarDadosAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var empresa = await empresaRepo.ObterPorIdAsync(tenant.EmpresaId);
        if (empresa is null) return NotFound();

        // Valida horário de corte no backend
        if (TimeOnly.FromDateTime(DateTime.Now) > empresa.HorarioCorte)
        {
            TempData["Erro"] = "O prazo para envio do pedido já encerrou.";
            await CarregarDadosAsync();
            return Page();
        }

        // Desserializa itens
        List<ItemPedidoDto>? itens;
        try
        {
            itens = JsonSerializer.Deserialize<List<ItemPedidoDto>>(PedidoJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            itens = null;
        }

        if (itens is null || itens.Count == 0)
        {
            TempData["Erro"] = "Nenhum item selecionado no pedido.";
            await CarregarDadosAsync();
            return Page();
        }

        var pedidoId = await pedidoRepo.SalvarLoteAsync(tenant.EmpresaId, itens);

        TempData["Sucesso"] = $"Pedido #{pedidoId} confirmado com {itens.Count} marmita(s)!";
        return RedirectToPage("/Pedido/Index");
    }

    private async Task CarregarDadosAsync()
    {
        var empresa     = await empresaRepo.ObterPorIdAsync(tenant.EmpresaId);
        HorarioCorte    = empresa?.HorarioCorte ?? new TimeOnly(10, 0);
        JaPediuHoje     = await pedidoRepo.JaPediuHojeAsync(tenant.EmpresaId);
        DataHoje        = DateTime.Today.ToString("dddd, dd/MM/yyyy");
        Funcionarios    = (await funcRepo.ListarAsync(tenant.EmpresaId)).Where(f => f.Ativo).ToList();

        var diaSemana   = (int)DateTime.Today.DayOfWeek;
        // DayOfWeek: 0=Dom,1=Seg...6=Sab  → nosso: 0=Seg...5=Sab
        var diaCardapio = diaSemana == 0 ? 4 : diaSemana - 1; // Dom cai em Sex como fallback
        var cardapio    = await cardapioRepo.ListarPorDiaAsync(diaCardapio, empresa?.RestauranteId ?? 1);
        Pratos          = cardapio.Where(c => c.Tipo == "prato").ToList();
        Acompanhamentos = cardapio.Where(c => c.Tipo == "acompanhamento").ToList();
    }
}
