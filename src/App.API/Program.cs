using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting application...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration)
                     .ReadFrom.Services(services));

    // Add services to the container.
    builder.Services.AddAPI(builder.Configuration);
    builder.Services.AddDAL(builder.Configuration);
    builder.Services.AddBusiness();

    var app = builder.Build();

    // Apply pending migrations and seed the database
    using var scope = app.Services.CreateScope();
    await AutomatedMigration.MigrateAsync(scope.ServiceProvider);

    app.UseAPI();

    app.Lifetime.ApplicationStarted.Register(() =>
    {
        var server = app.Services.GetRequiredService<IServer>();
        var addresses = server.Features.Get<IServerAddressesFeature>()?.Addresses;
        if (addresses is null || addresses.Count == 0)
            return;

        foreach (var address in addresses)
            Log.Information("Now listening at {Url}", address);

        var baseUrl = addresses.FirstOrDefault(a =>
            a.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            a.StartsWith("https://", StringComparison.OrdinalIgnoreCase));
        if (baseUrl is not null)
            Log.Information("Swagger UI: {SwaggerUrl}", $"{baseUrl.TrimEnd('/')}/swagger");
    });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly.");
}
finally
{
    Log.CloseAndFlush();
}
