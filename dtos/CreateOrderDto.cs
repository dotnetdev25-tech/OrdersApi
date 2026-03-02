namespace OrdersApi.Dtos;

public record CreateOrderDto
{
    public int customer_id { get; init; }
    public int total_amount { get; init; }

    public int order_type_id { get; init; }
    public int order_status_id { get; set; }
    public int payment_method_id { get; set; }  
    public int fulfillment_method_id { get; set; }

}
  
