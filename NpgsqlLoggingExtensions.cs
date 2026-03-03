using Microsoft.AspNetCore.Mvc;
using Npgsql;
using OrdersApi.Dtos;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text;
using RazorLight;
using System.Diagnostics;


public static class NpgsqlLoggingExtensions
{
    public static async Task<NpgsqlDataReader> ExecuteReaderWithLoggingAsync(
        this NpgsqlCommand cmd,
        ILogger logger)
    {
        var sql = cmd.CommandText;
        var parameters = cmd.Parameters
            .Cast<NpgsqlParameter>()
            .ToDictionary(p => p.ParameterName, p => p.Value);

        var sw = Stopwatch.StartNew();

        try
        {
            logger.LogInformation("SQL: {Sql} | Params: {@Params}", sql, parameters);

            var reader = await cmd.ExecuteReaderAsync();

            sw.Stop();
            logger.LogInformation("SQL executed in {Elapsed} ms", sw.ElapsedMilliseconds);

            return reader;
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex,
                "SQL FAILED: {Sql} | Params: {@Params} | Time: {Elapsed} ms",
                sql, parameters, sw.ElapsedMilliseconds);

            throw;
        }
    }
}
