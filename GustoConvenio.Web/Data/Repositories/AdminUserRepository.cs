using Dapper;
using GustoConvenio.Web.Models;

namespace GustoConvenio.Web.Data.Repositories;

public class AdminUserRepository(DbConnectionFactory db) : IAdminUserRepository
{
    public async Task<AdminUser?> ObterPorEmailAsync(string email)
    {
        using var conn = db.Create();
        return await conn.QueryFirstOrDefaultAsync<AdminUser>(
            "SELECT * FROM admin_users WHERE email = @Email AND ativo = 1",
            new { Email = email });
    }

    public async Task<int> CriarAsync(AdminUser user)
    {
        using var conn = db.Create();
        return await conn.ExecuteScalarAsync<int>("""
            INSERT INTO admin_users (nome, email, senha_hash, ativo, restaurante_id, is_super_admin)
            VALUES (@Nome, @Email, @SenhaHash, @Ativo, @RestauranteId, @IsSuperAdmin);
            SELECT LAST_INSERT_ID();
            """, user);
    }
}
