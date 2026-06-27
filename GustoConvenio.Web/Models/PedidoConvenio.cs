namespace GustoConvenio.Web.Models;

public class PedidoConvenio
{
    public int      Id          { get; set; }
    public string   NomeEmpresa { get; set; } = "";
    public string   Status      { get; set; } = "";
    public TimeSpan HorarioPedido { get; set; }
    public List<ItemPedidoDto> Itens { get; set; } = [];
    public decimal  ValorTotal  => Itens.Sum(i => i.Preco);
}
