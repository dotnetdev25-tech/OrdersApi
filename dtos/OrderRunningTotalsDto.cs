public class OrderRunningTotalsDto
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }

    public decimal running_total { get; set; }
    public decimal GlobalRunningTotal { get; set; }
    public int OrderNumberForCustomer { get; set; }
}
