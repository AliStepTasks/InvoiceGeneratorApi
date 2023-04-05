using InvoiceGeneratorApi.Data;
using InvoiceGeneratorApi.DTO;
using InvoiceGeneratorApi.DTO.Pagination;
using InvoiceGeneratorApi.Enums;
using InvoiceGeneratorApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PdfSharpCore;
using PdfSharpCore.Pdf;
using TheArtOfDev.HtmlRenderer.PdfSharp;

namespace InvoiceGeneratorApi.Services;

public class InvoiceService : IServiceInvoice
{
    private readonly InvoiceApiDbContext _context;

    public InvoiceService(InvoiceApiDbContext context)
    {
        _context = context;
    }

    public async Task<InvoiceDTO> ChangeInvoiceStatus(int id, InvoiceStatus invoiceStatus)
    {
        var invoice = _context.Invoices.FirstOrDefault(i => i.Id == id);

        if (invoice is null)
        {
            return null;
        }

        invoice.Status = invoiceStatus;
        invoice = _context.Invoices.Update(invoice).Entity;
        await _context.SaveChangesAsync();

        return InvoiceToInvoiceDto(invoice);
    }

    /// <summary>
    /// Create a new invoice
    /// </summary>
    /// <param name="invoiceDTO"></param>
    /// <returns></returns>
    public async Task<InvoiceDTO> CreateInvoice(InvoiceDTO invoiceDTO)
    {
        var invoice = InvoiceDtoToInvoice(invoiceDTO);

        invoice.UpdatedAt = DateTimeOffset.Now;
        invoice.DeletedAt = DateTimeOffset.MinValue;
        
        invoice = _context.Invoices.Add(invoice).Entity;
        await _context.SaveChangesAsync();

        return InvoiceToInvoiceDto(invoice);
    }

    /// <summary>
    /// Delete invoice by Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<object> DeleteInvoice(int id)
    {
        var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == id);

        if (invoice is null || invoice.Status != InvoiceStatus.Sent ||
            invoice.Status != InvoiceStatus.Received || invoice.Status != InvoiceStatus.Rejected)
        {
            return null;
        }

        _context.Invoices.Remove(invoice);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<InvoiceDTO> EditInvoice(InvoiceDTO invoiceDTO)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Get invoice by Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<InvoiceDTO> GetInvoice(int id)
    {
        var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == id);

        if (invoice is null)
        {
            return null;
        }

        return InvoiceToInvoiceDto(invoice);
    }

    /// <summary>
    /// Gett all invoices
    /// </summary>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <param name="search"></param>
    /// <param name="orderBy"></param>
    /// <returns></returns>
    public async Task<PaginationDTO<InvoiceDTO>> GetInvoices(int page, int pageSize, string? search, OrderBy? orderBy)
    {
        IQueryable<Invoice> query = _context.Invoices;

        // Search
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(i => i.Comment.Contains(search));
        }

        // Sorting
        if (OrderBy.Ascending == orderBy)
        {
            query = query.OrderBy(i => _context.Customers.Count(c => c.Id == i.CustomerId));
        }
        else if (OrderBy.Descending == orderBy)
        {
            query = query.OrderByDescending(i => _context.Customers.Count(c => c.Id == i.CustomerId));
        }

        // Pagination

        var invoiceList = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

        var invoiceDtoList = invoiceList.Select(i => InvoiceToInvoiceDto(i));

        var paginatedList = new PaginationDTO<InvoiceDTO>
        (
            invoiceDtoList,
            new PaginatinoMeta(page, pageSize, _context.Invoices.Count())
        );

        return paginatedList;
    }

    /// <summary>
    /// Converts Invoice to InvoiceDTO
    /// </summary>
    /// <param name="invoice"></param>
    /// <returns></returns>
    private InvoiceDTO InvoiceToInvoiceDto(Invoice invoice)
    {
        var invoiceDTO = new InvoiceDTO
        {
            Id = invoice.Id,
            CustomerId = invoice.CustomerId,
            StartDate = invoice.StartDate,
            EndDate = invoice.EndDate,
            Rows = invoice.Rows,
            TotalSum = invoice.TotalSum,
            Comment = invoice.Comment,
            Status = invoice.Status,
            CreatedAt = invoice.CreatedAt
        };

        return invoiceDTO;
    }

    /// <summary>
    /// Converts InvoiceDTO to Invoice
    /// </summary>
    /// <param name="invoiceDTO"></param>
    /// <returns></returns>
    private Invoice InvoiceDtoToInvoice(InvoiceDTO invoiceDTO)
    {
        var invoice = new Invoice
        {
            Id = invoiceDTO.Id,
            CustomerId = invoiceDTO.CustomerId,
            StartDate = invoiceDTO.StartDate,
            EndDate = invoiceDTO.EndDate,
            Rows = invoiceDTO.Rows,
            TotalSum = invoiceDTO.TotalSum,
            Comment = invoiceDTO.Comment,
            Status = invoiceDTO.Status,
            CreatedAt = invoiceDTO.CreatedAt
        };

        return invoice;
    }

    public async Task<(byte[], string, string)> GenerateInvoicePDF(int id)
    {
        var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == id);

        if(invoice is null)
        {
            return (null, null, null);
        }

        var document = new PdfDocument();
        var htmlContent = HtmlContentGet(invoice);
        PdfGenerator.AddPdfPages(document, htmlContent, PageSize.A4);
        byte[]? response = null;
        using (MemoryStream ms = new MemoryStream())
        {
            document.Save(ms);
            response = ms.ToArray();
        }

        string Filename = "InvoiceNo_" + Guid.NewGuid().ToString() + ".pdf";
        return (response, "application/pdf", Filename);
    }

    public Task<IActionResult> GenerateInvoiceDOCx(int id)
    {
        throw new NotImplementedException();
    }

    private string HtmlContentGet(Invoice invoice)
    {

        string htmlcontent = "<div style='width:100%; text-align:center'>";
        //htmlcontent += "<img style='width:80px;height:80%' src='" + imgeurl + "'   />";
        //htmlcontent += "<h2>" + copies[i] + "</h2>";
        htmlcontent += "<h2>Welcome to Nihira Techiees</h2>";



        //if (header != null)
        //{
        //    htmlcontent += "<h2> Invoice No:" + header.InvoiceNo + " & Invoice Date:" + header.InvoiceDate + "</h2>";
        //    htmlcontent += "<h3> Customer : " + header.CustomerName + "</h3>";
        //    htmlcontent += "<p>" + header.DeliveryAddress + "</p>";
        //    htmlcontent += "<h3> Contact : 9898989898 & Email :ts@in.com </h3>";
        //    htmlcontent += "<div>";
        //}



        htmlcontent += "<table style ='width:100%; border: 1px solid #000'>";
        htmlcontent += "<thead style='font-weight:bold'>";
        htmlcontent += "<tr>";
        htmlcontent += "<td style='border:1px solid #000'> Product Code </td>";
        htmlcontent += "<td style='border:1px solid #000'> Description </td>";
        htmlcontent += "<td style='border:1px solid #000'>Qty</td>";
        htmlcontent += "<td style='border:1px solid #000'>Price</td >";
        htmlcontent += "<td style='border:1px solid #000'>Total</td>";
        htmlcontent += "</tr>";
        htmlcontent += "</thead >";

        htmlcontent += "<tbody>";
        //if (detail != null && detail.Count > 0)
        //{
        //    detail.ForEach(item =>
        //    {
        //        htmlcontent += "<tr>";
        //        htmlcontent += "<td>" + item.ProductCode + "</td>";
        //        htmlcontent += "<td>" + item.ProductName + "</td>";
        //        htmlcontent += "<td>" + item.Qty + "</td >";
        //        htmlcontent += "<td>" + item.SalesPrice + "</td>";
        //        htmlcontent += "<td> " + item.Total + "</td >";
        //        htmlcontent += "</tr>";
        //    });
        //}
        htmlcontent += "</tbody>";

        htmlcontent += "</table>";
        htmlcontent += "</div>";

        htmlcontent += "<div style='text-align:right'>";
        htmlcontent += "<h1> Summary Info </h1>";
        htmlcontent += "<table style='border:1px solid #000;float:right' >";
        htmlcontent += "<tr>";
        htmlcontent += "<td style='border:1px solid #000'> Summary Total </td>";
        htmlcontent += "<td style='border:1px solid #000'> Summary Tax </td>";
        htmlcontent += "<td style='border:1px solid #000'> Summary NetTotal </td>";
        htmlcontent += "</tr>";
        htmlcontent += "<tr>";
        htmlcontent += "<td style='border: 1px solid #000'></td>"; //+ header.Total + " </td>";
        htmlcontent += "<td style='border: 1px solid #000'></td>"; // + header.Tax + "</td>";
        htmlcontent += "<td style='border: 1px solid #000'></td>"; // + header.NetTotal + "</td>";
        htmlcontent += "</tr>";
        htmlcontent += "</table>";
        htmlcontent += "</div>";

        htmlcontent += "</div>";

        return htmlcontent;
    }
}