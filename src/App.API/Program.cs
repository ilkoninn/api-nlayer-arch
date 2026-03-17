var builder = WebApplication.CreateBuilder(args);

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
