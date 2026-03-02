namespace OrdersApi.Dtos;

public record OrderItemDto
{

public int order_id { get; init; }
public int Id { get; init; }
    
public string product_name { get; init; } = null!;
    public int Quantity { get; init; }
    public decimal unit_price { get; init; }
    public decimal line_total{get;init;}
    public decimal running_total{get;init;}


   
}

