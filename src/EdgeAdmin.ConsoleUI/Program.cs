using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile("appsettings.Development.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var baseUrl = config["ServiceBaseUrl"] ?? "http://localhost:5000";
using var http = new HttpClient { BaseAddress = new Uri(baseUrl) };

static void PrintMenu()
{
    Console.WriteLine();
    Console.WriteLine("Console UI (Service Client)");
    Console.WriteLine("1. List all devices");
    Console.WriteLine("2. Show total devices");
    Console.WriteLine("3. Show total users");
    Console.WriteLine("4. Show last used device by user id");
    Console.WriteLine("5. Create device");
    Console.WriteLine("6. Update device");
    Console.WriteLine("7. Delete device");
    Console.WriteLine("8. Get device by id");
    Console.WriteLine("9. Show user created/updated dates");
    Console.WriteLine("10. Mark user inactive if stale");
    Console.WriteLine("0. Exit");
    Console.Write("> ");
}

while (true)
{
    PrintMenu();
    var input = Console.ReadLine()?.Trim();

    if (input == "0") break;

    if (input == "1")
    {
        await PrintResponseAsync(await http.GetAsync("/api/devices"));
        continue;
    }

    if (input == "2")
    {
        await PrintResponseAsync(await http.GetAsync("/api/devices/total"));
        continue;
    }

    if (input == "3")
    {
        await PrintResponseAsync(await http.GetAsync("/api/users/total"));
        continue;
    }

    if (input == "4")
    {
        var userId = ReadLong("User id: ");
        await PrintResponseAsync(await http.GetAsync($"/api/users/{userId}/last-device"));
        continue;
    }

    if (input == "5")
    {
        Console.Write("Public device id (e.g. dev_123): ");
        var guid = Console.ReadLine() ?? "";
        Console.Write("Name: ");
        var name = Console.ReadLine() ?? "";
        var deviceTypeId = ReadLong("Device type id: ");
        var ownerUserId = ReadLong("Owner user id: ");

        var payload = new { publicDeviceId = guid, name, deviceTypeId, ownerUserId };
        await PrintResponseAsync(await http.PostAsJsonAsync("/api/devices", payload));
        continue;
    }

    if (input == "6")
    {
        Console.Write("Target public device id: ");
        var targetGuid = Console.ReadLine() ?? "";
        Console.Write("New public device id: ");
        var newGuid = Console.ReadLine() ?? "";
        Console.Write("Name: ");
        var name = Console.ReadLine() ?? "";

        var payload = new { publicDeviceId = newGuid, name };
        await PrintResponseAsync(await http.PutAsJsonAsync($"/api/devices/by-public-id/{targetGuid}", payload));
        continue;
    }

    if (input == "7")
    {
        var id = ReadLong("Device id: ");
        await PrintResponseAsync(await http.DeleteAsync($"/api/devices/{id}"));
        continue;
    }

    if (input == "8")
    {
        var id = ReadLong("Device id: ");
        await PrintResponseAsync(await http.GetAsync($"/api/devices/{id}"));
        continue;
    }

    if (input == "9")
    {
        var userId = ReadLong("User id: ");
        await PrintResponseAsync(await http.GetAsync($"/api/users/{userId}/dates"));
        continue;
    }

    if (input == "10")
    {
        var userId = ReadLong("User id: ");
        await PrintResponseAsync(await http.PostAsync($"/api/users/{userId}/mark-inactive-if-stale", null));
        continue;
    }

    Console.WriteLine("Unknown option. Choose 0-10.");
}

static long ReadLong(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        var raw = Console.ReadLine();
        if (long.TryParse(raw, out var parsed) && parsed > 0) return parsed;
        Console.WriteLine("Enter a positive integer.");
    }
}

static async Task PrintResponseAsync(HttpResponseMessage response)
{
    var json = await response.Content.ReadAsStringAsync();

    if (string.IsNullOrWhiteSpace(json))
    {
        Console.WriteLine($"HTTP {(int)response.StatusCode}");
        return;
    }

    try
    {
        using var doc = JsonDocument.Parse(json);
        var pretty = JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine($"HTTP {(int)response.StatusCode}");
        Console.WriteLine(pretty);
    }
    catch
    {
        Console.WriteLine($"HTTP {(int)response.StatusCode}");
        Console.WriteLine(json);
    }
}
