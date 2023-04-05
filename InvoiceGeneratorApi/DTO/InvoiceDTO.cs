using InvoiceGeneratorApi.Enums;
using InvoiceGeneratorApi.Models;
using Newtonsoft.Json;

namespace InvoiceGeneratorApi.DTO;

public class InvoiceDTO
{
    [JsonIgnore]
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public InvoiceRow[] Rows { get; set; }
    public decimal TotalSum { get; set; }
    public string? Comment { get; set; }
    public InvoiceStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}