using EdgeAdmin.DAL.Db;
using EdgeAdmin.DAL.Repositories;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

static string GetConnectionString()
{
    var config = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: true)
        .AddJsonFile("appsettings.Development.json", optional: true)
        .AddEnvironmentVariables()
        .Build();

    return config.GetConnectionString("Db")
           ?? config["DB_CONNECTION_STRING"]
           ?? throw new InvalidOperationException("Missing connection string (ConnectionStrings:Db or DB_CONNECTION_STRING).");
}

var cs = GetConnectionString();

var factory = new MySqlConnectionFactory(new DbSettings { ConnectionString = cs });
var repo = new DeviceRepository(factory);

static async Task<int> GetCountAsync(MySqlConnectionFactory factory, string sql, CancellationToken ct = default)
{
    await using var conn = factory.Create();
    await conn.OpenAsync(ct);

    await using var cmd = new MySqlCommand(sql, conn);
    var scalar = await cmd.ExecuteScalarAsync(ct);
    return Convert.ToInt32(scalar);
}

static void PrintMenu()
{
    Console.WriteLine();
    Console.WriteLine("Console UI");
    Console.WriteLine("1. List all devices");
    Console.WriteLine("2. Show total devices");
    Console.WriteLine("3. Show total users");
    Console.WriteLine("0. Exit");
    Console.Write("> ");
}

while (true)
{
    PrintMenu();
    var input = Console.ReadLine()?.Trim();

    if (string.Equals(input, "0", StringComparison.OrdinalIgnoreCase))
        break;

    if (string.Equals(input, "1", StringComparison.OrdinalIgnoreCase))
    {
        var devices = await repo.GetAllAsync();
        Console.WriteLine($"Found {devices.Count} device rows:");
        foreach (var d in devices)
            Console.WriteLine($"{d.DeviceId} | {d.DisplayName} | {d.DeviceGuid} | {d.CreatedAtUtc:O}");
        continue;
    }

    if (string.Equals(input, "2", StringComparison.OrdinalIgnoreCase))
    {
        var totalDevices = await GetCountAsync(factory, "SELECT COUNT(*) FROM devices;");
        Console.WriteLine($"Total devices: {totalDevices}");
        continue;
    }

    if (string.Equals(input, "3", StringComparison.OrdinalIgnoreCase))
    {
        var totalUsers = await GetCountAsync(factory, "SELECT COUNT(*) FROM user_accounts;");
        Console.WriteLine($"Total users: {totalUsers}");
        continue;
    }

    Console.WriteLine("Unknown option. Choose 1, 2, 3, or 0.");
}
