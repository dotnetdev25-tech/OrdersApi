using OrdersApi.Dtos;
public sealed class OrdersRepository
{
    private readonly PgDb _db;

    public OrdersRepository(PgDb db)
    {
        _db = db;
    }

    public async Task<List<OrderDto>> GetAllAsync()
    {
        const string sql = """
            SELECT id, customer_id, order_date, order_type_id, total_amount
            FROM orders
            ORDER BY id;
        """;

        await using var cmd = _db.CreateCommand(sql);
        await using var reader = await cmd.ExecuteReaderAsync();

        var results = new List<OrderDto>();

        while (await reader.ReadAsync())
        {
            results.Add(new OrderDto
            {
                OrderId = reader.GetInt32(0),
                CustomerId = reader.GetInt32(1),
                OrderDate = reader.GetDateTime(2),
                order_type_id = reader.GetInt32(3),
                OrderAmount = reader.GetInt32(4),
                CustomerName = string.Empty,
                DimensonCode = string.Empty,
                RunningTotal = 0
            });
        }

        return results;
    }
}
