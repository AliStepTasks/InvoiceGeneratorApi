using InvoiceGeneratorApi.DTO;

namespace InvoiceGeneratorApi.Interfaces;

public interface IReportService
{
    Task<byte[]> GenerateCustomerReport(List<CustomerDTO> customers);
}