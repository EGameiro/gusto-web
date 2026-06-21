namespace GustoConvenio.Web.Models;

public class CardapioItem
{
    public int Id { get; set; }
    public int DiaSemana { get; set; }   // 0=Seg … 5=Sab
    public string Tipo { get; set; } = "";  // "prato" | "acompanhamento"
    public string Nome { get; set; } = "";
    public bool Ativo { get; set; } = true;
    public int Ordem { get; set; }
    public int RestauranteId { get; set; } = 1;
}
