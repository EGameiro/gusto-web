using Dapper;
using GustoConvenio.Web.Models;

namespace GustoConvenio.Web.Data.Repositories;

public class RestauranteRepository(DbConnectionFactory db) : IRestauranteRepository
{
    public async Task<List<Restaurante>> ListarTodosAsync()
    {
        using var conn = db.Create();
        var rows = await conn.QueryAsync<Restaurante>(
            "SELECT * FROM restaurantes ORDER BY nome");
        return rows.ToList();
    }

    public async Task<Restaurante?> ObterPorIdAsync(int id)
    {
        using var conn = db.Create();
        return await conn.QueryFirstOrDefaultAsync<Restaurante>(
            "SELECT * FROM restaurantes WHERE id = @Id", new { Id = id });
    }

    public async Task<int> CriarAsync(Restaurante r)
    {
        using var conn = db.Create();
        return await conn.ExecuteScalarAsync<int>("""
            INSERT INTO restaurantes (nome, slug, telefone, email, endereco, ativo)
            VALUES (@Nome, @Slug, @Telefone, @Email, @Endereco, @Ativo);
            SELECT LAST_INSERT_ID();
            """, r);
    }

    public async Task AtualizarAsync(Restaurante r)
    {
        using var conn = db.Create();
        await conn.ExecuteAsync("""
            UPDATE restaurantes SET
                nome     = @Nome,
                slug     = @Slug,
                telefone = @Telefone,
                email    = @Email,
                endereco = @Endereco,
                ativo    = @Ativo
            WHERE id = @Id
            """, r);
    }
}
