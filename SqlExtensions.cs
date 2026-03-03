using Npgsql;

public static class SqlExtensions
{
    public static NpgsqlCommand CreateLoggedCommand(
        this NpgsqlDataSource db,
        string sql,
        ILogger logger)
    {
        logger.LogInformation("SQL: {Sql}", sql);
        return db.CreateCommand(sql);
    }

    public static void LogParameters(this NpgsqlCommand cmd, ILogger logger)
    {
        foreach (NpgsqlParameter p in cmd.Parameters)
        {
            logger.LogInformation("  Param {Name} = {Value}", p.ParameterName, p.Value);
        }
    }
}
