using InvoiceGeneratorApi.Enums;
using InvoiceGeneratorApi.Models;
using System.Text.Json.Serialization;

namespace InvoiceGeneratorApi.DTO;

public class InvoiceDTO
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public InvoiceRowDTO[]? Rows { get; set; }
    public decimal TotalSum { get; set; }
    public string? Comment { get; set; }
    public InvoiceStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}