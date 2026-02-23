using EdgeAdmin.Business.Abstractions;
using EdgeAdmin.Business.Results;
using EdgeAdmin.Business.Services;
using EdgeAdmin.DAL.Db;
using EdgeAdmin.DAL.Repositories;
using Microsoft.Extensions.Configuration;

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
IDeviceQueryService service = new DeviceQueryService(repo);

static void PrintMenu()
{
    Console.WriteLine();
    Console.WriteLine("Console UI");
    Console.WriteLine("1. List all devices");
    Console.WriteLine("2. Show total devices");
    Console.WriteLine("3. Show total users");
    Console.WriteLine("4. Show last used device by user id");
    Console.WriteLine("0. Exit");
    Console.Write("> ");
}

static void PrintError(ServiceError? error)
{
    if (error is null)
    {
        Console.WriteLine("Request failed with an unknown error.");
        return;
    }

    Console.WriteLine($"Error [{error.Code}]: {error.Message}");
}

while (true)
{
    PrintMenu();
    var input = Console.ReadLine()?.Trim();

    if (string.Equals(input, "0", StringComparison.OrdinalIgnoreCase))
        break;

    if (string.Equals(input, "1", StringComparison.OrdinalIgnoreCase))
    {
        var result = await service.GetAllDevicesAsync();
        if (!result.IsSuccess)
        {
            PrintError(result.Error);
            continue;
        }

        var devices = result.Value ?? [];
        Console.WriteLine($"Found {devices.Count} device rows:");
        foreach (var d in devices)
            Console.WriteLine($"{d.Id} | {d.Name} | {d.PublicDeviceId} | {d.CreatedAtUtc:O}");
        continue;
    }

    if (string.Equals(input, "2", StringComparison.OrdinalIgnoreCase))
    {
        var result = await service.GetTotalDevicesAsync();
        if (!result.IsSuccess)
        {
            PrintError(result.Error);
            continue;
        }

        Console.WriteLine($"Total devices: {result.Value}");
        continue;
    }

    if (string.Equals(input, "3", StringComparison.OrdinalIgnoreCase))
    {
        var result = await service.GetTotalUsersAsync();
        if (!result.IsSuccess)
        {
            PrintError(result.Error);
            continue;
        }

        Console.WriteLine($"Total users: {result.Value}");
        continue;
    }

    if (string.Equals(input, "4", StringComparison.OrdinalIgnoreCase))
    {
        Console.Write("User id: ");
        var rawUserId = Console.ReadLine()?.Trim();
        if (!long.TryParse(rawUserId, out var userId))
        {
            Console.WriteLine("Please enter a valid integer user id.");
            continue;
        }

        var result = await service.GetLastUsedDeviceForUserAsync(userId);
        if (!result.IsSuccess)
        {
            PrintError(result.Error);
            continue;
        }

        var row = result.Value!;
        Console.WriteLine($"User {row.UserId} last used device {row.DeviceId} ({row.DeviceName}) at {row.LastUsedAtUtc:O}");
        continue;
    }

    Console.WriteLine("Unknown option. Choose 1, 2, 3, 4, or 0.");
}
