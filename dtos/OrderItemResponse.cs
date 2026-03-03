using OrdersApi.Dtos;
namespace OrdersApi.Dtos;
public record OrderItemResponse
{
	public int Id { get; init; }
	public string ProductName { get; init; } = null!;
	public int Quantity { get; init; }
	public decimal UnitPrice { get; init; }

	public OrderItemResponse(int id, string productName, int quantity, decimal unitPrice)
	{
		Id = id;
		ProductName = productName;
		Quantity = quantity;
		UnitPrice = unitPrice;
	}

	public void Deconstruct(out int id, out string productName, out int quantity, out decimal unitPrice) => (id, productName, quantity, unitPrice) = (Id, ProductName, Quantity, UnitPrice);
}
