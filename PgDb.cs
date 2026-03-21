using Npgsql;
public sealed class PgDb
{
    private readonly NpgsqlDataSource _dataSource;

    public PgDb(string connectionString)
    {
        _dataSource = NpgsqlDataSource.Create(connectionString);
        
    }

    public NpgsqlCommand CreateCommand(string sql)
        => _dataSource.CreateCommand(sql);
}
