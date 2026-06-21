using MySqlConnector;

namespace GustoConvenio.Web.Data;

public class DbConnectionFactory(IConfiguration config)
{
    private readonly string _connString = config.GetConnectionString("DefaultConnection")!;

    public MySqlConnection Create() => new(_connString);
}
