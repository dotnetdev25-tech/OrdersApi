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
/////////
//dotnet add package Serilog.AspNetCore
//dotnet add package Serilog.Sinks.Console
//dotnet add package Serilog.Settings.Configuration
builder.Host.UseSerilog((context, services, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));
// Configure database connection pooling
var connectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddSingleton<NpgsqlDataSource>(
    _ => new NpgsqlDataSourceBuilder(connectionString).Build());

builder.Services.AddProblemDetails();

// In the pipeline
//app.UseExceptionHandler();    // catches unhandled exceptions
//app.UseStatusCodePages();      // turns bare 404s/500s into ProblemDetails JSON
//app.UseExceptionHandler(errorApp =>
//{
//    errorApp.Run(async context =>
//    {
 //       var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        
 //       var problemDetails = exception switch
  //      {
//            NotFoundException => new ProblemDetails
//            {
 //               Status = 404,
 //               Title = "Resource Not Found",
 //               Detail = exception.Message
 //           },
 //           ValidationException ve => new ProblemDetails
  //          {
  //              Status = 400,
  //              Title = "Validation Failed",
  //              Extensions = { ["errors"] = ve.Errors }
  //          },
  //          _ => new ProblemDetails
   //         {
   //             Status = 500,
   //             Title = "Internal Server Error"
                // Detail intentionally omitted — no stack traces to clients
   //         }
   //     };

    //    context.Response.StatusCode = problemDetails.Status ?? 500;
    //    await context.Response.WriteAsJsonAsync(problemDetails);
   // });
//});

    ////// ANOTHER WAY TO CONFIGURE SERILOG

//Log.Logger = new LoggerConfiguration()
 //   .Enrich.WithUtcTimestamp()
//    .WriteTo.Console(outputTemplate:
 //       "{UtcTimestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}")
 //   .CreateLogger();
//
/////
#pragma warning disable CS8604 // Possible null reference argument.
builder.Services.AddSingleton(new PgDb(connectionString));
#pragma warning restore CS8604 // Possible null reference argument.
builder.Services.AddScoped<OrdersRepository>();
//builder.Services.AddScoped<CustomersRepository>();
//builder.Services.AddScoped<OrderItemsRepository>();


// Configure CORS for Blazor frontend
// 1. Define the policy
builder.Services.AddCors(options => {
    options.AddPolicy("LocalDevPolicy", policy => {
        policy.WithOrigins("http://localhost:5000")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
// Configure QuestPDF license
QuestPDF.Settings.License = LicenseType.Community;

var app = builder.Build();

// Configure middleware
app.UseCors("LocalDevPolicy");
app.UseSerilogRequestLogging(); // replaces default noisy request logs
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
logger.LogCritical("{Timestamp} serverstarted!!!!!!!!", DateTime.Now);

app.Run();
