using Dapper;
using GustoConvenio.Web.Models;

namespace GustoConvenio.Web.Data.Repositories;

public class EmpresaRepository(DbConnectionFactory db) : IEmpresaRepository
{
    private const string SelectBase =
        "SELECT *, horario_corte AS horario_corte_raw FROM empresas_convenio";

    public async Task<Empresa?> ObterPorEmailAsync(string email)
    {
        using var conn = db.Create();
        return await conn.QueryFirstOrDefaultAsync<Empresa>(
            $"{SelectBase} WHERE email = @Email AND ativo = 1",
            new { Email = email });
    }

    public async Task<Empresa?> ObterPorIdAsync(int id)
    {
        using var conn = db.Create();
        return await conn.QueryFirstOrDefaultAsync<Empresa>(
            $"{SelectBase} WHERE id = @Id",
            new { Id = id });
    }

    public async Task<List<Empresa>> ListarTodasAsync(int restauranteId)
    {
        using var conn = db.Create();
        var rows = await conn.QueryAsync<Empresa>(
            $"{SelectBase} WHERE restaurante_id = @RestauranteId ORDER BY nome_empresa",
            new { RestauranteId = restauranteId });
        return rows.ToList();
    }

    public async Task<int> CriarAsync(Empresa e)
    {
        using var conn = db.Create();
        return await conn.ExecuteScalarAsync<int>("""
            INSERT INTO empresas_convenio
                (nome_empresa, numero_whatsapp, endereco_padrao, horario_padrao,
                 forma_pgto_padrao, ativo, email, senha_hash, horario_corte)
            VALUES
                (@NomeEmpresa, @NumeroWhatsapp, @EnderecoPadrao, @HorarioPadrao,
                 @FormaPgtoPadrao, @Ativo, @Email, @SenhaHash, @HorarioCorteRaw);
            SELECT LAST_INSERT_ID();
            """, e);
    }

    public async Task AtualizarAsync(Empresa e)
    {
        using var conn = db.Create();
        await conn.ExecuteAsync("""
            UPDATE empresas_convenio SET
                nome_empresa      = @NomeEmpresa,
                numero_whatsapp   = @NumeroWhatsapp,
                endereco_padrao   = @EnderecoPadrao,
                email             = @Email,
                senha_hash        = @SenhaHash,
                horario_corte     = @HorarioCorteRaw,
                ativo             = @Ativo
            WHERE id = @Id
            """, e);
    }
}
