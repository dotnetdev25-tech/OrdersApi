using Microsoft.Extensions.Logging;
using Npgsql;
using QuestPDF.Infrastructure;
using Microsoft.Extensions.Logging;
using System.IO;
using Serilog;


using OrdersApi.Dtos;

var builder = WebApplication.CreateBuilder(args);
// make sure the directory exists
Directory.CreateDirectory("logs");

// Configure logging
//builder.Logging.ClearProviders();
//builder.Logging.AddConsole();
//builder.Logging.Addfil

// Configure API services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure database connection pooling
var connectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddSingleton<NpgsqlDataSource>(
    _ => new NpgsqlDataSourceBuilder(connectionString).Build());

#pragma warning disable CS8604 // Possible null reference argument.
builder.Services.AddSingleton(new PgDb(connectionString));
#pragma warning restore CS8604 // Possible null reference argument.
builder.Services.AddScoped<OrdersRepository>();
//builder.Services.AddScoped<CustomersRepository>();
//builder.Services.AddScoped<OrderItemsRepository>();


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

// Configure Serilog *before* building the app
// Configure Serilog before building the app
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.File(
        path: "logs/log-.txt",          // log files in ./logs
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,      // keep last 7 days
        shared: true                    // allow shared access, no buffered writes
    )
    .CreateLogger();

builder.Host.UseSerilog();

var app = builder.Build();

// Configure middleware
app.UseCors("BlazorPolicy");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
var logger = LoggerFactory.Create(cfg =>
{
    cfg.AddConsole();
}).CreateLogger("startup");
logger.LogCritical("{Timestamp} serverstarted", DateTime.Now);

app.Run();
