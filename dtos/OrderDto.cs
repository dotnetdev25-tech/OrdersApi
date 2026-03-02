namespace OrdersApi.Dtos;
public record OrderDto
{
    public int OrderId { get; init; }
    public string CustomerName{get;init;}  = null!;
    public int CustomerId { get; init; } = 0!;
     public DateTime OrderDate { get; init; }
     public int OrderAmount { get; init; }  = 0!;
      public int order_type_id {get; init;}
      public string DimensonCode {get; init;}  = null!;

      public int RunningTotal { get; init; }  = 0!;
    
}

   
