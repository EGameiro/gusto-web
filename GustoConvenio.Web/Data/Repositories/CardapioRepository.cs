using Dapper;
using GustoConvenio.Web.Models;

namespace GustoConvenio.Web.Data.Repositories;

public class CardapioRepository(DbConnectionFactory db) : ICardapioRepository
{
    public async Task<List<CardapioItem>> ListarPorDiaAsync(int diaSemana, int restauranteId)
    {
        using var conn = db.Create();
        var rows = await conn.QueryAsync<CardapioItem>(
            "SELECT * FROM cardapio_web WHERE dia_semana = @DiaSemana AND restaurante_id = @RestauranteId AND ativo = 1 ORDER BY tipo, ordem, nome",
            new { DiaSemana = diaSemana, RestauranteId = restauranteId });
        return rows.ToList();
    }

    public async Task<List<CardapioItem>> ListarTodosAsync(int restauranteId)
    {
        using var conn = db.Create();
        var rows = await conn.QueryAsync<CardapioItem>(
            "SELECT * FROM cardapio_web WHERE restaurante_id = @RestauranteId ORDER BY dia_semana, tipo, ordem, nome",
            new { RestauranteId = restauranteId });
        return rows.ToList();
    }

    public async Task<int> CriarAsync(CardapioItem item)
    {
        using var conn = db.Create();
        return await conn.ExecuteScalarAsync<int>(
            "INSERT INTO cardapio_web (dia_semana, tipo, nome, ativo, ordem, restaurante_id) VALUES (@DiaSemana, @Tipo, @Nome, @Ativo, @Ordem, @RestauranteId); SELECT LAST_INSERT_ID();",
            item);
    }

    public async Task ExcluirAsync(int id, int restauranteId)
    {
        using var conn = db.Create();
        await conn.ExecuteAsync("DELETE FROM cardapio_web WHERE id = @Id AND restaurante_id = @RestauranteId",
            new { Id = id, RestauranteId = restauranteId });
    }
}
