using EdgeAdmin.Business.Abstractions;
using EdgeAdmin.Business.DTOs;
using EdgeAdmin.Business.Results;
using EdgeAdmin.Business.Services;
using EdgeAdmin.DAL.Abstractions;
using EdgeAdmin.DAL.Db;
using EdgeAdmin.DAL.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Hosting notes:
// - Platform: ASP.NET Core (Kestrel) on .NET 8.
// - Local run: dotnet run --project .\src\EdgeAdmin.Service\EdgeAdmin.Service.csproj
// - By default this listens on localhost (HTTP/HTTPS) using launch profile settings.
// - You can host this on Azure App Service, IIS, Docker, or any VM running dotnet runtime.

var resolvedConfig = new ConfigurationBuilder()
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddConfiguration(builder.Configuration)
    .AddJsonFile("appsettings.Development.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var cs = resolvedConfig.GetConnectionString("Db")
         ?? resolvedConfig["DB_CONNECTION_STRING"]
         ?? throw new InvalidOperationException("Missing connection string (ConnectionStrings:Db or DB_CONNECTION_STRING).");

builder.Services.AddSingleton(new MySqlConnectionFactory(new DbSettings { ConnectionString = cs }));
builder.Services.AddScoped<DeviceRepository>();
builder.Services.AddScoped<IDeviceQueryRepository>(sp => sp.GetRequiredService<DeviceRepository>());
builder.Services.AddScoped<ICrudRepository<EdgeAdmin.Shared.Models.Device, long>>(sp => sp.GetRequiredService<DeviceRepository>());
builder.Services.AddScoped<IUserAccountRepository, UserAccountRepository>();
builder.Services.AddScoped<IDeviceQueryService, DeviceQueryService>();

var app = builder.Build();

app.MapGet("/api/devices", async (IDeviceQueryService service, CancellationToken ct) =>
{
    var result = await service.GetAllDevicesAsync(ct);
    return ToHttpResult(result);
});

app.MapGet("/api/devices/{id:long}", async (long id, IDeviceQueryService service, CancellationToken ct) =>
{
    var result = await service.GetDeviceByIdAsync(id, ct);
    return ToHttpResult(result);
});

app.MapPost("/api/devices", async (CreateDeviceRequestDto request, IDeviceQueryService service, CancellationToken ct) =>
{
    var result = await service.CreateDeviceAsync(request, ct);
    return ToHttpResult(result);
});

app.MapPut("/api/devices/by-public-id/{publicDeviceId}", async (string publicDeviceId, UpdateDeviceRequestDto request, IDeviceQueryService service, CancellationToken ct) =>
{
    var merged = new UpdateDeviceRequestDto
    {
        TargetPublicDeviceId = publicDeviceId,
        PublicDeviceId = request.PublicDeviceId,
        Name = request.Name
    };

    var result = await service.UpdateDeviceAsync(merged, ct);
    return ToHttpResult(result);
});

app.MapDelete("/api/devices/{id:long}", async (long id, IDeviceQueryService service, CancellationToken ct) =>
{
    var result = await service.DeleteDeviceAsync(id, ct);
    return ToHttpResult(result);
});

app.MapGet("/api/devices/total", async (IDeviceQueryService service, CancellationToken ct) =>
{
    var result = await service.GetTotalDevicesAsync(ct);
    return ToHttpResult(result);
});

app.MapGet("/api/users/total", async (IDeviceQueryService service, CancellationToken ct) =>
{
    var result = await service.GetTotalUsersAsync(ct);
    return ToHttpResult(result);
});

app.MapGet("/api/users/{userId:long}/last-device", async (long userId, IDeviceQueryService service, CancellationToken ct) =>
{
    var result = await service.GetLastUsedDeviceForUserAsync(userId, ct);
    return ToHttpResult(result);
});

app.MapGet("/api/users/{userId:long}/dates", async (long userId, IDeviceQueryService service, CancellationToken ct) =>
{
    var result = await service.GetUserAccountDatesAsync(userId, ct);
    return ToHttpResult(result);
});

app.MapPost("/api/users/{userId:long}/mark-inactive-if-stale", async (long userId, IDeviceQueryService service, CancellationToken ct) =>
{
    var result = await service.MarkUserInactiveIfStaleAsync(userId, ct);
    return ToHttpResult(result);
});

app.Run();

static IResult ToHttpResult<T>(ServiceResult<T> result)
{
    if (result.IsSuccess)
    {
        return Results.Ok(result);
    }

    var code = result.Error?.Code ?? ErrorCodes.Infrastructure;
    var statusCode = code switch
    {
        ErrorCodes.Validation => StatusCodes.Status400BadRequest,
        ErrorCodes.NotFound => StatusCodes.Status404NotFound,
        ErrorCodes.SchemaMismatch => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status500InternalServerError
    };

    return Results.Json(result, statusCode: statusCode);
}
