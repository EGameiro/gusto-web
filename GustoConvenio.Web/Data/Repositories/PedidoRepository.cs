using Dapper;
using GustoConvenio.Web.Models;

namespace GustoConvenio.Web.Data.Repositories;

public class PedidoRepository(DbConnectionFactory db) : IPedidoRepository
{
    public async Task<bool> JaPediuHojeAsync(int empresaId)
    {
        using var conn = db.Create();
        var count = await conn.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM pedidos WHERE tipo = 'convenio' AND empresa_id = @EmpresaId AND data_pedido = CURDATE()",
            new { EmpresaId = empresaId });
        return count > 0;
    }

    public async Task<int> SalvarLoteAsync(int empresaId, List<ItemPedidoDto> itens)
    {
        using var conn = db.Create();
        await conn.OpenAsync();
        using var tx = await conn.BeginTransactionAsync();

        var pedidoId = await conn.ExecuteScalarAsync<int>("""
            INSERT INTO pedidos
                (tipo, empresa_id, numero_whatsapp, data_pedido, horario_pedido, status, impresso)
            VALUES
                ('convenio', @EmpresaId, '', CURDATE(), CURTIME(), 'pendente', 0);
            SELECT LAST_INSERT_ID();
            """, new { EmpresaId = empresaId }, tx);

        foreach (var item in itens)
        {
            await conn.ExecuteAsync("""
                INSERT INTO itens_pedido
                    (pedido_id, nome_pessoa, mistura, tamanho, acomp_1, acomp_2, observacoes, valor_unitario)
                VALUES
                    (@PedidoId, @Nome, @Prato, @Tamanho, @Acomp1, @Acomp2, null, @Preco)
                """,
                new
                {
                    PedidoId = pedidoId,
                    item.Nome,
                    item.Prato,
                    item.Tamanho,
                    item.Acomp1,
                    item.Acomp2,
                    Preco = item.Preco,
                }, tx);
        }

        await tx.CommitAsync();
        return pedidoId;
    }

    public async Task<List<PedidoResumo>> ListarPorEmpresaAsync(int empresaId, int mes, int ano)
    {
        using var conn = db.Create();
        var rows = await conn.QueryAsync<PedidoResumo>("""
            SELECT
                p.id            AS Id,
                p.data_pedido   AS DataPedido,
                p.status        AS Status,
                COUNT(i.id)                     AS TotalMarmitas,
                COALESCE(SUM(i.valor_unitario), 0) AS TotalValor
            FROM pedidos p
            LEFT JOIN itens_pedido i ON i.pedido_id = p.id
            WHERE p.tipo = 'convenio'
              AND p.empresa_id = @EmpresaId
              AND MONTH(p.data_pedido) = @Mes
              AND YEAR(p.data_pedido)  = @Ano
            GROUP BY p.id, p.data_pedido, p.status
            ORDER BY p.data_pedido DESC
            """, new { EmpresaId = empresaId, Mes = mes, Ano = ano });
        return rows.ToList();
    }

    public async Task<(PedidoResumo resumo, List<ItemPedidoDto> itens)?> ObterDetalhesAsync(int pedidoId, int empresaId)
    {
        using var conn = db.Create();

        var resumo = await conn.QueryFirstOrDefaultAsync<PedidoResumo>("""
            SELECT p.id AS Id, p.data_pedido AS DataPedido, p.status AS Status,
                   COUNT(i.id) AS TotalMarmitas,
                   COALESCE(SUM(i.valor_unitario), 0) AS TotalValor
            FROM pedidos p
            LEFT JOIN itens_pedido i ON i.pedido_id = p.id
            WHERE p.id = @PedidoId AND p.empresa_id = @EmpresaId
            GROUP BY p.id, p.data_pedido, p.status
            """, new { PedidoId = pedidoId, EmpresaId = empresaId });

        if (resumo is null) return null;

        var itens = (await conn.QueryAsync<ItemPedidoDto>("""
            SELECT 0 AS FuncionarioId, nome_pessoa AS Nome,
                   mistura AS Prato, tamanho AS Tamanho,
                   acomp_1 AS Acomp1, acomp_2 AS Acomp2,
                   COALESCE(valor_unitario, 0) AS Preco
            FROM itens_pedido WHERE pedido_id = @PedidoId
            ORDER BY nome_pessoa
            """, new { PedidoId = pedidoId })).ToList();

        return (resumo, itens);
    }

    public async Task<(string NomeEmpresa, PedidoResumo resumo, List<ItemPedidoDto> itens)?> ObterDetalhesAdminAsync(int pedidoId)
    {
        using var conn = db.Create();

        var resumo = await conn.QueryFirstOrDefaultAsync<PedidoResumo>("""
            SELECT p.id AS Id, p.data_pedido AS DataPedido, p.status AS Status,
                   COUNT(i.id) AS TotalMarmitas,
                   COALESCE(SUM(i.valor_unitario), 0) AS TotalValor
            FROM pedidos p
            LEFT JOIN itens_pedido i ON i.pedido_id = p.id
            WHERE p.id = @PedidoId
            GROUP BY p.id, p.data_pedido, p.status
            """, new { PedidoId = pedidoId });

        if (resumo is null) return null;

        var nomeEmpresa = await conn.ExecuteScalarAsync<string>("""
            SELECT e.nome_empresa FROM pedidos p
            JOIN empresas_convenio e ON e.id = p.empresa_id
            WHERE p.id = @PedidoId
            """, new { PedidoId = pedidoId }) ?? "";

        var itens = (await conn.QueryAsync<ItemPedidoDto>("""
            SELECT 0 AS FuncionarioId, nome_pessoa AS Nome,
                   mistura AS Prato, tamanho AS Tamanho,
                   acomp_1 AS Acomp1, acomp_2 AS Acomp2,
                   COALESCE(valor_unitario, 0) AS Preco
            FROM itens_pedido WHERE pedido_id = @PedidoId
            ORDER BY nome_pessoa
            """, new { PedidoId = pedidoId })).ToList();

        return (nomeEmpresa, resumo, itens);
    }

    public async Task<List<(string NomeEmpresa, int TotalMarmitas, string Status, int PedidoId)>> ListarTodosHojeAsync()
    {
        using var conn = db.Create();
        var rows = await conn.QueryAsync("""
            SELECT e.nome_empresa AS NomeEmpresa,
                   COUNT(i.id)   AS TotalMarmitas,
                   p.status      AS Status,
                   p.id          AS PedidoId
            FROM pedidos p
            JOIN empresas_convenio e ON e.id = p.empresa_id
            LEFT JOIN itens_pedido i ON i.pedido_id = p.id
            WHERE p.tipo = 'convenio' AND p.data_pedido = CURDATE()
            GROUP BY p.id, e.nome_empresa, p.status
            ORDER BY e.nome_empresa
            """);
        return rows.Select(r => (
            NomeEmpresa:   (string)r.NomeEmpresa,
            TotalMarmitas: (int)r.TotalMarmitas,
            Status:        (string)r.Status,
            PedidoId:      (int)r.PedidoId
        )).ToList();
    }

    public async Task<List<PedidoWhatsApp>> ListarWhatsAppHojeAsync(int restauranteId, string? statusFiltro = null)
    {
        using var conn = db.Create();
        var sql = """
            SELECT
                p.id                AS Id,
                p.numero_whatsapp   AS NumeroWhatsApp,
                COALESCE(c.nome, p.numero_whatsapp) AS NomeCliente,
                p.status            AS Status,
                p.horario_pedido    AS HorarioPedido,
                p.endereco_entrega  AS EnderecoEntrega,
                p.hora_retirada     AS HoraRetirada,
                i.mistura           AS Mistura,
                i.tamanho           AS Tamanho,
                i.acomp_1           AS Acomp1,
                i.acomp_2           AS Acomp2,
                i.observacoes       AS Observacoes,
                COALESCE(i.valor_unitario, 0) AS ValorUnitario
            FROM pedidos p
            LEFT JOIN itens_pedido i ON i.pedido_id = p.id
            LEFT JOIN clientes c ON c.numero_whatsapp = p.numero_whatsapp
            WHERE p.tipo = 'individual'
              AND p.restaurante_id = @RestauranteId
              AND p.data_pedido = CURDATE()
            """;

        if (!string.IsNullOrEmpty(statusFiltro))
            sql += " AND p.status = @StatusFiltro";

        sql += " ORDER BY p.horario_pedido DESC";

        return (await conn.QueryAsync<PedidoWhatsApp>(sql,
            new { RestauranteId = restauranteId, StatusFiltro = statusFiltro })).ToList();
    }

    public async Task AtualizarStatusWhatsAppAsync(int pedidoId, string novoStatus)
    {
        using var conn = db.Create();
        await conn.ExecuteAsync(
            "UPDATE pedidos SET status = @Status WHERE id = @Id AND tipo = 'individual'",
            new { Status = novoStatus, Id = pedidoId });
    }

    public async Task<(int EmPreparo, int Saiu, int Entregues)> TotaisWhatsAppHojeAsync(int restauranteId)
    {
        using var conn = db.Create();
        var rows = await conn.QueryAsync("""
            SELECT status, COUNT(*) AS Total
            FROM pedidos
            WHERE tipo = 'individual'
              AND restaurante_id = @RestauranteId
              AND data_pedido = CURDATE()
            GROUP BY status
            """, new { RestauranteId = restauranteId });

        int emPreparo = 0, saiu = 0, entregues = 0;
        foreach (var r in rows)
        {
            switch ((string)r.status)
            {
                case "preparo":  emPreparo = (int)r.Total; break;
                case "saiu":     saiu      = (int)r.Total; break;
                case "entregue": entregues = (int)r.Total; break;
            }
        }
        return (emPreparo, saiu, entregues);
    }
}
