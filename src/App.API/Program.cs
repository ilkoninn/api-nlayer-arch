Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting application...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration)
                     .ReadFrom.Services(services)
                     .Enrich.WithProperty("ApiBaseUrl", context.Configuration["Api:BaseUrl"] ?? "")
                     .Enrich.WithProperty("SwaggerUrl", context.Configuration["Api:SwaggerUrl"] ?? ""));

    // Add services to the container.
    builder.Services.AddAPI(builder.Configuration);
    builder.Services.AddDAL(builder.Configuration);
    builder.Services.AddBusiness();

    var app = builder.Build();

    // Apply pending migrations and seed the database
    using var scope = app.Services.CreateScope();
    await AutomatedMigration.MigrateAsync(scope.ServiceProvider);

    app.UseAPI();

    var apiUrl = app.Configuration["Api:BaseUrl"];
    var swaggerUrl = app.Configuration["Api:SwaggerUrl"];
    Log.Information("API URL: {ApiUrl}", apiUrl);
    Log.Information("Swagger: {SwaggerUrl}", swaggerUrl);

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
