using InvoiceGeneratorApi.DTO;
using InvoiceGeneratorApi.DTO.Pagination;
using InvoiceGeneratorApi.Enums;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceGeneratorApi.Services;

public interface IServiceInvoice
{
    Task<InvoiceDTO> CreateInvoice(InvoiceDTO invoiceDTO);
    Task<InvoiceDTO> EditInvoice(InvoiceDTO invoiceDTO);
    Task<InvoiceDTO> ChangeInvoiceStatus(int id, InvoiceStatus invoiceStatus);
    Task<object> DeleteInvoice(int id);
    Task<InvoiceDTO> GetInvoice(int id);
    Task<PaginationDTO<InvoiceDTO>> GetInvoices(int page, int pageSize, string? search, OrderBy? orderBy);
    Task<(byte[], string, string)> GenerateInvoicePDF(int id);
    Task<IActionResult> GenerateInvoiceDOCx(int id);
}