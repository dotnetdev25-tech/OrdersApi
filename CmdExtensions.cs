using Npgsql;
public static class CmdExtensions
{
    public static NpgsqlCommand WithParam(this NpgsqlCommand cmd, string name, object value)
    {
        cmd.Parameters.AddWithValue(name, value);
        return cmd;
    }
}
