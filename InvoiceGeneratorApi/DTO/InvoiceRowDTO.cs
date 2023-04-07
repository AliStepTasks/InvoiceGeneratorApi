using System.Text.Json.Serialization;

namespace InvoiceGeneratorApi.DTO;

public class InvoiceRowDTO
{
    public int Id { get; set; }
    public string Service { get; set; }
    public decimal Quantity { get; set; }
    public decimal Amount { get; set; }
    public decimal Sum { get; set; }
}