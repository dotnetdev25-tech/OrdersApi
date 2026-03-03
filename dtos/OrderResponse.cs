namespace OrdersApi.Dtos;

public record OrderResponse
{
	public int Id { get; init; }
	public DateTime OrderDate { get; init; }
	public int order_type_id{get;init;}
	public List<OrderItemResponse> Items { get; init; } = null!;


}
