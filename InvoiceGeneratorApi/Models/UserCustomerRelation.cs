namespace InvoiceGeneratorApi.Models;

public class UserCustomerRelation
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CustomerId { get; set; }
}