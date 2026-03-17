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
