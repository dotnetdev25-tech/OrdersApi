public record CreateOrderItemDto
{
    public int order_id { get; init; }
    public string product_name { get; set; }

    public int Quantity { get; init; }
    public int unit_price { get; set; }
    
}
