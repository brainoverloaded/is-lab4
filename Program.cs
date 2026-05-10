using IsLabApp.Models;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register in-memory notes service
builder.Services.AddSingleton<NotesStore>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Эндпоинт /health
app.MapGet("/health", () =>
    Results.Json(new { status = "ok", timestamp = DateTime.UtcNow }));

// Эндпоинт /version
app.MapGet("/version", (IConfiguration config) =>
    Results.Json(new
    {
        name = config["App:Name"] ?? "IsLabApp",
        version = config["App:Version"] ?? "unknown"
    }));

// Эндпоинт /db/ping
app.MapGet("/db/ping", async (IConfiguration config, ILogger<Program> logger) =>
{
    try
    {
        var connStr = config.GetConnectionString("Mssql");

        if (string.IsNullOrWhiteSpace(connStr))
        {
            return Results.Json(new
            {
                status = "error",
                message = "Connection string not configured"
            }, statusCode: 500);
        }

        // Маскируем пароль в логах
        var maskedConnStr = connStr;
        if (connStr.Contains("Password="))
        {
            var parts = connStr.Split("Password=");
            if (parts.Length > 1)
            {
                var passwordPart = parts[1].Split(";")[0];
                maskedConnStr = connStr.Replace(passwordPart, "***");
            }
        }

        logger.LogInformation("Attempting DB connection with: {ConnStr}", maskedConnStr);

        // Пробуем подключиться к БД
        await using var connection = new SqlConnection(connStr);
        await connection.OpenAsync();

        return Results.Json(new
        {
            status = "ok",
            message = "Database connection successful",
            server = connection.DataSource,
            database = connection.Database
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "DB ping failed");
        return Results.Json(new
        {
            status = "error",
            message = ex.Message,
            hint = "SQL Server may not be deployed yet"
        }, statusCode: 500);
    }
});

app.Run();
