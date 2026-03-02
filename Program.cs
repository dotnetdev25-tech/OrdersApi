using Microsoft.Extensions.Logging;
using Npgsql;
using QuestPDF.Infrastructure;

using OrdersApi.Dtos;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Configure API services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure database connection pooling
var connectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddSingleton<NpgsqlDataSource>(
    _ => new NpgsqlDataSourceBuilder(connectionString).Build());

// Configure CORS for Blazor frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorPolicy", policy =>
        policy.WithOrigins("https://localhost:7000")
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// Configure QuestPDF license
QuestPDF.Settings.License = LicenseType.Community;

var app = builder.Build();

// Configure middleware
app.UseCors("BlazorPolicy");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
