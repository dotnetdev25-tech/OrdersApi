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
            SELECT id, customer_id, order_date, status
            FROM orders
            ORDER BY id;
        """;

        await using var cmd = _db.CreateCommand(sql);
        await using var reader = await cmd.ExecuteReaderAsync();

        var results = new List<OrderDto>();
// @todo:m
        while (await reader.ReadAsync())
        {
       //     results.Add(new OrderResponse(
        //        reader.GetInt32(0),
        //        reader.GetInt32(1),
        //        reader.GetDateTime(2),
        //        reader.GetString(3)
         //   ));
        }

        return results;
    }
}
