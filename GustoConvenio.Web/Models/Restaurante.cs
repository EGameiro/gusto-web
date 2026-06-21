namespace GustoConvenio.Web.Models;

public class Restaurante
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Slug { get; set; } = "";
    public string? Telefone { get; set; }
    public string? Email { get; set; }
    public string? Endereco { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; }
}
