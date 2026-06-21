namespace GustoConvenio.Web.Models;

public class AdminUser
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string Email { get; set; } = "";
    public string SenhaHash { get; set; } = "";
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; }
    public int RestauranteId { get; set; } = 1;
    public bool IsSuperAdmin { get; set; } = false;
}
