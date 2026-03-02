namespace OrdersApi.Dtos;

public record CreateCustomerDto
{
    public string Name { get; init; } = null!;
    public string Email { get; init; } = null!;

 }
