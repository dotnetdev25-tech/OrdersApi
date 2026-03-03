namespace OrdersApi.Dtos;
using OrdersApi.Dtos;
public record CustomerResponse
{
	public int customerId { get; init; }
	public string customerName { get; init; } = null!;
	public string customerType { get; init; } = null!;
	public string customerEmail { get; init; } = null!;
	public List<OrderResponse> Orders { get; init; } = null!;
	

}
