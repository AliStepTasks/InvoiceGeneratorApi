using InvoiceGeneratorApi.DTO;
using InvoiceGeneratorApi.Models;

namespace InvoiceGeneratorApi.Interfaces;

public interface IReportService
{
    Task<byte[]> GenerateCustomerReport(List<CustomerDTO> customers, List<Invoice> invoices);
    Task<byte[]> GenerateInvoiceReport(IEnumerable<InvoiceDTO> invoices, IEnumerable<Customer> customers);
}