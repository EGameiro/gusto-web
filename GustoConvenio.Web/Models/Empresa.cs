namespace GustoConvenio.Web.Models;

public class Empresa
{
    public int Id { get; set; }
    public string NomeEmpresa { get; set; } = "";
    public string NumeroWhatsapp { get; set; } = "";
    public string? EnderecoPadrao { get; set; }
    public string? HorarioPadrao { get; set; }
    public string? FormaPgtoPadrao { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; }
    public string? Email { get; set; }
    public string? SenhaHash { get; set; }
    public TimeSpan HorarioCorteRaw { get; set; } = new(10, 0, 0);
    public TimeOnly HorarioCorte => TimeOnly.FromTimeSpan(HorarioCorteRaw);
    public int RestauranteId { get; set; } = 1;
}
