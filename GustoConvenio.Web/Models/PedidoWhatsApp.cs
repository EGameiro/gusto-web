namespace GustoConvenio.Web.Models;

public class PedidoWhatsApp
{
    public int       Id              { get; set; }
    public string    NumeroWhatsApp  { get; set; } = "";
    public string    NomeCliente     { get; set; } = "";
    public string    Status          { get; set; } = "";
    public TimeOnly  HorarioPedido   { get; set; }
    public string?   EnderecoEntrega { get; set; }
    public string?   HoraRetirada    { get; set; }
    public string?   Mistura         { get; set; }
    public string?   Tamanho         { get; set; }
    public string?   Acomp1          { get; set; }
    public string?   Acomp2          { get; set; }
    public string?   Observacoes     { get; set; }
    public decimal   ValorUnitario   { get; set; }
}
