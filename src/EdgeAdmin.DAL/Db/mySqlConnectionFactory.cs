using MySqlConnector;

namespace EdgeAdmin.DAL.Db;

public sealed class MySqlConnectionFactory
{
    private readonly string _cs;

    public MySqlConnectionFactory(DbSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.ConnectionString))
            throw new ArgumentException("ConnectionString is required.", nameof(settings));

        _cs = settings.ConnectionString;
    }

    public MySqlConnection Create() => new MySqlConnection(_cs);
}

