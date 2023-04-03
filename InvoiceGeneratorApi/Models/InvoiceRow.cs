namespace InvoiceGeneratorApi.Models;

public class InvoiceRow
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public string Service { get; set; }
    public decimal Quantity { get; set; }
    public decimal Amount { get; set; }
    public decimal Sum { get; set; }
}