///// change to see in git sees change
using Npgsql;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

using OrdersApi.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add console logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// set up npgsqldatasource which pools connections so this will be passed to controllers
builder.Services.AddSingleton<NpgsqlDataSource>(sp => { var cs = builder.Configuration.GetConnectionString("Default"); return new NpgsqlDataSourceBuilder(cs).Build(); });

// Set the license to Community
QuestPDF.Settings.License = LicenseType.Community;
// 1. Define the policy
builder.Services.AddCors(options => {
    options.AddPolicy("BlazorPolicy", policy => 
        policy.WithOrigins("https://localhost:7000") // Your Blazor URL
              .AllowAnyMethod()
              .AllowAnyHeader());
});


var app = builder.Build();
// 2. Use it
app.UseCors("BlazorPolicy");
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();


app.Run();
