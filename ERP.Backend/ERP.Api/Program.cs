using Azure.Monitor.OpenTelemetry.AspNetCore;
using ERP.Api.Data;
using ERP.Api.Modules.Authentication.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    connectionString = "Host=localhost;Port=5432;Database=erp_db;Username=postgres;Password=postgres";
}

builder.Services.AddDbContext<ErpDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var frontendUrl = builder.Configuration["Frontend:Url"];
if (string.IsNullOrWhiteSpace(frontendUrl))
{
    frontendUrl = "http://localhost:5500";
}

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(frontendUrl).AllowAnyHeader().AllowAnyMethod());
});

if (!string.IsNullOrWhiteSpace(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
{
    builder.Services.AddOpenTelemetry().UseAzureMonitor();
}

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseMiddleware<BearerTokenMiddleware>();
app.MapControllers();
app.MapGet("/", () => Results.Ok(new { name = "ERP API", status = "ok" }));

app.Run();
