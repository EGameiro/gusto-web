using Dapper;
using GustoConvenio.Web.Models;

namespace GustoConvenio.Web.Data.Repositories;

public class FuncionarioRepository(DbConnectionFactory db) : IFuncionarioRepository
{
    public async Task<List<Funcionario>> ListarAsync(int empresaId)
    {
        using var conn = db.Create();
        var rows = await conn.QueryAsync<Funcionario>(
            "SELECT * FROM funcionarios WHERE empresa_id = @EmpresaId ORDER BY nome",
            new { EmpresaId = empresaId });
        return rows.ToList();
    }

    public async Task<Funcionario?> ObterPorIdAsync(int id, int empresaId)
    {
        using var conn = db.Create();
        return await conn.QueryFirstOrDefaultAsync<Funcionario>(
            "SELECT * FROM funcionarios WHERE id = @Id AND empresa_id = @EmpresaId",
            new { Id = id, EmpresaId = empresaId });
    }

    public async Task<int> CriarAsync(Funcionario f)
    {
        using var conn = db.Create();
        return await conn.ExecuteScalarAsync<int>(
            "INSERT INTO funcionarios (empresa_id, nome, ativo) VALUES (@EmpresaId, @Nome, @Ativo); SELECT LAST_INSERT_ID();",
            f);
    }

    public async Task AtualizarAsync(Funcionario f)
    {
        using var conn = db.Create();
        await conn.ExecuteAsync(
            "UPDATE funcionarios SET nome = @Nome, ativo = @Ativo WHERE id = @Id AND empresa_id = @EmpresaId",
            f);
    }

    public async Task DesativarAsync(int id, int empresaId)
    {
        using var conn = db.Create();
        await conn.ExecuteAsync(
            "UPDATE funcionarios SET ativo = 0 WHERE id = @Id AND empresa_id = @EmpresaId",
            new { Id = id, EmpresaId = empresaId });
    }
}
