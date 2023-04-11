using InvoiceGeneratorApi.DTO;

namespace InvoiceGeneratorApi.PdfRelated.Models;

public class InvoiceModel
{
    public InvoiceDTO InvoiceDto { get; set; }
    public string InvoiceNumber { get; set; }
    public DateTime DueDate { get; set; }
    public Address SellerAddress { get; set; }
    public Address CustomerAddress { get; set; }
}