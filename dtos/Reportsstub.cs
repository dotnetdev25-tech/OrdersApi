namespace OrdersApi.Dtos;
public record Reportsstub
{
    public int customerid { get; init; }
public string customerName { get; init; } = null!;
public string report_type {get;init;} = null!;
    public string customerEmail { get; init; } = null!;
    public string customerType { get; init; } = null!;
    
}
