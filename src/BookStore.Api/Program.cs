using BookStore.Api.Features.Catalog;
using BookStore.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

const string DevWebCorsPolicy = "DevWeb";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();

// The "Testing" environment (WebApplicationFactory) registers its own provider.
if (!builder.Environment.IsEnvironment("Testing"))
{
    var connectionString = builder.Configuration.GetConnectionString("BookStore")
        ?? throw new InvalidOperationException(
            "Connection string 'BookStore' is not configured. Set ConnectionStrings__BookStore or use appsettings.Development.json.");
    builder.Services.AddDbContext<BookStoreDbContext>(options => options.UseNpgsql(connectionString));
}

builder.Services.AddScoped<ICatalogService, CatalogService>();

builder.Services.AddCors(options => options.AddPolicy(DevWebCorsPolicy, policy =>
    policy.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.UseCors(DevWebCorsPolicy);
}

await DbSeeder.InitialiseAsync(app.Services);

app.MapControllers();

app.MapGet("/health", async (BookStoreDbContext db, CancellationToken cancellationToken) =>
    await db.Database.CanConnectAsync(cancellationToken)
        ? Results.Ok(new { status = "Healthy" })
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable));

app.Run();

/// <summary>Entry point marker so WebApplicationFactory&lt;Program&gt; can host the API in tests.</summary>
public partial class Program { }
