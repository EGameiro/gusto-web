namespace GustoConvenio.Web.Models;

public record ItemPedidoDto(
    long    FuncionarioId,
    string  Nome,
    string  Prato,
    string  Tamanho,
    string? Acomp1,
    string? Acomp2,
    decimal Preco = 0m
);

public static class Tamanhos
{
    public static readonly Dictionary<string, decimal> Precos = new()
    {
        ["Mini"]      = 21.90m,
        ["Normal"]    = 23.90m,
        ["Executiva"] = 24.90m,
        ["Churrasco"] = 27.90m,
    };
}
