using InvoiceGeneratorApi.DTO;
using InvoiceGeneratorApi.Models;

namespace InvoiceGeneratorApi.Interfaces;

public interface IReportService
{
    Task<byte[]> GenerateCustomerReport(List<CustomerDTO> customers, List<Invoice> invoices);
    Task<byte[]> GenerateDoneWorkReport(List<InvoiceDTO> invoices, List<Customer> customers);
    Task<byte[]> GenerateInvoiceReport(List<InvoiceDTO> invoices, List<Customer> customers);
}