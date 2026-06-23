using GustoConvenio.Web.Models;

namespace GustoConvenio.Web.Data.Repositories;

public class PedidoResumo
{
    public int      Id            { get; set; }
    public DateTime DataPedido    { get; set; }
    public int      TotalMarmitas { get; set; }
    public decimal  TotalValor    { get; set; }
    public string   Status        { get; set; } = "";
}

public interface IPedidoRepository
{
    Task<bool>             JaPediuHojeAsync(int empresaId);
    Task<int>              SalvarLoteAsync(int empresaId, List<ItemPedidoDto> itens);
    Task<List<PedidoResumo>> ListarPorEmpresaAsync(int empresaId, int mes, int ano);
    Task<(PedidoResumo resumo, List<ItemPedidoDto> itens)?> ObterDetalhesAsync(int pedidoId, int empresaId);

    // Admin — sem filtro de tenant
    Task<List<(string NomeEmpresa, int TotalMarmitas, string Status, int PedidoId)>> ListarTodosHojeAsync();
    Task<(string NomeEmpresa, PedidoResumo resumo, List<ItemPedidoDto> itens)?> ObterDetalhesAdminAsync(int pedidoId);

    // Pedidos WhatsApp (individuais)
    Task<List<PedidoWhatsApp>> ListarWhatsAppHojeAsync(int restauranteId, string? statusFiltro = null);
    Task AtualizarStatusWhatsAppAsync(int pedidoId, string novoStatus);
    Task<(int EmPreparo, int Saiu, int Entregues)> TotaisWhatsAppHojeAsync(int restauranteId);
}
