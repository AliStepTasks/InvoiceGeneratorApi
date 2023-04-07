using Bogus.DataSets;
using InvoiceGeneratorApi.Data;
using InvoiceGeneratorApi.DTO;
using InvoiceGeneratorApi.DTO.Pagination;
using InvoiceGeneratorApi.Enums;
using InvoiceGeneratorApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

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
        var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == id);
        invoice!.Rows = _context.InvoiceRows
            .Where(r => r.InvoiceId == invoice.Id)
            .Select(r => DtoAndReverseConverter.InvoiceRowToInvoiceRowDto(r))
            .ToArray();

        if (invoice is null)
        {
            return null!;
        }

        invoice.Status = invoiceStatus;
        invoice = _context.Invoices.Update(invoice).Entity;
        await _context.SaveChangesAsync();

        return DtoAndReverseConverter.InvoiceToInvoiceDto(invoice);
    }

    /// <summary>
    /// Create a new invoice
    /// </summary>
    /// <param name="invoiceDTO"></param>
    /// <returns></returns>
    public async Task<InvoiceDTO> CreateInvoice(InvoiceDTO invoiceDTO)
    {
        var invoice = DtoAndReverseConverter.InvoiceDtoToInvoice(invoiceDTO);

        invoice.UpdatedAt = DateTimeOffset.Now;
        invoice.DeletedAt = DateTimeOffset.MinValue;

        for (int i = 0; i < invoice.Rows.Length; i++)
            invoice.Rows[i].Sum = invoice.Rows[i].Quantity * invoice.Rows[i].Amount;

        invoice.TotalSum = invoice.Rows.Sum(r => r.Sum);
        invoice = _context.Invoices.Add(invoice).Entity;
        await _context.SaveChangesAsync();

        foreach (var dtoRow in invoice.Rows)
        {
            var row = DtoAndReverseConverter.InvoiceRowDtoToInvoiceRow(dtoRow);
            row.InvoiceId = invoice.Id;
            _context.InvoiceRows.Add(row);
        }

        await _context.SaveChangesAsync();

        return DtoAndReverseConverter.InvoiceToInvoiceDto(invoice);
    }

    /// <summary>
    /// Delete invoice by Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<InvoiceDTO> DeleteInvoice(int id)
    {
        var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == id);

        if (invoice is null || invoice.Status == InvoiceStatus.Sent ||
            invoice.Status == InvoiceStatus.Received || invoice.Status == InvoiceStatus.Rejected)
        {
            return null!;
        }

        invoice.Rows = _context.InvoiceRows
            .Where(r => r.InvoiceId == invoice.Id)
            .Select(r => DtoAndReverseConverter.InvoiceRowToInvoiceRowDto(r))
            .ToArray();

        _context.Invoices.Remove(invoice);
        await _context.SaveChangesAsync();

        return DtoAndReverseConverter.InvoiceToInvoiceDto(invoice);
    }

    public async Task<InvoiceDTO> EditInvoice(
        int invoiceId, int? customerId, DateTimeOffset? startDate,
        DateTimeOffset? endDate, string? comment, InvoiceStatus? status)
    {
        var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice is null)
        {
            return null!;
        }

        invoice.CustomerId = (int)(customerId is not null ? customerId : invoice.CustomerId);
        invoice.StartDate = (DateTimeOffset)(startDate is not null ? startDate : invoice.StartDate);
        invoice.EndDate = (DateTimeOffset)(endDate is not null ? endDate : invoice.EndDate);
        invoice.Comment = comment is not null ? comment : invoice.Comment;
        invoice.Status = (InvoiceStatus)(status is not null ? status : invoice.Status);

        invoice = _context.Invoices.Update(invoice).Entity;
        await _context.SaveChangesAsync();

        var invoiceRowsDto = _context.InvoiceRows
            .Where(r => r.InvoiceId == invoice.Id)
            .Select(r => DtoAndReverseConverter.InvoiceRowToInvoiceRowDto(r));
        invoice.Rows = invoiceRowsDto.ToArray();

        return DtoAndReverseConverter.InvoiceToInvoiceDto(invoice);
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
            return null!;
        }

        var invoiceRowsDto = _context.InvoiceRows
            .Where(r => r.InvoiceId == invoice.Id)
            .Select(r => DtoAndReverseConverter.InvoiceRowToInvoiceRowDto(r));
        invoice.Rows = invoiceRowsDto.ToArray();

        return DtoAndReverseConverter.InvoiceToInvoiceDto(invoice);
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
            query = query.Where(i => i.Comment!.Contains(search));
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

        var invoiceDtoList = invoiceList.Select(i => DtoAndReverseConverter.InvoiceToInvoiceDto(i)).ToList();
        for (int i = 0; i < invoiceDtoList.Count(); i++)
        {
            var invoiceRowsDto = _context.InvoiceRows
                .Where(r => r.InvoiceId == invoiceDtoList[i].Id)
                .Select(r => DtoAndReverseConverter.InvoiceRowToInvoiceRowDto(r));
            invoiceDtoList[i].Rows = invoiceRowsDto.ToArray();
        }

        var paginatedList = new PaginationDTO<InvoiceDTO>
        (
            invoiceDtoList,
            new PaginatinoMeta(page, pageSize, _context.Invoices.Count())
        );

        return paginatedList;
    }

    public async Task<(MemoryStream, string, string)> GenerateInvoicePDF(int id)
    {
        return (null!, null!, null!);
    }

    public Task<IActionResult> GenerateInvoiceDOCx(int id)
    {
        throw new NotImplementedException();
    }

    private string HtmlContentGet(Invoice invoice)
    {
        var customer = _context.Customers.FirstOrDefault(c => c.Id == invoice.CustomerId);


        //string htmlcontent = "<head><title>Invoice</title>";
        //htmlcontent += "<link rel='stylesheet' href='wwwRoot/invoiceStyle.css'></head>";

        //htmlcontent += "<div class=\"container\">";
        //htmlcontent += "<div class=\"row invoice-header px-3 py-2\">";
        //htmlcontent += "<div class=\"col-4\">";
        //htmlcontent += "<p>Schofire LLC</p>";
        //htmlcontent += "<h1>INVOICE</h1>";
        //htmlcontent += "</div>";
        //htmlcontent += "<div class=\"col-4 text-right\">";
        //htmlcontent += "<p>+994 (050) 499 97 44</p>";
        //htmlcontent += "<p>aguliyev45@gmail.com</p>";
        //htmlcontent += "</div>";
        //htmlcontent += "<div class=\"col-4 text-right\">";
        //htmlcontent += "<p>3522 Zafarano Dr #F1\r\n</p>";
        //htmlcontent += "<p>Santa Fe, New York, 87505</p>";
        //htmlcontent += "</div>";
        //htmlcontent += "</div>";
        //htmlcontent += "<div class=\"invoice-content row px-5 pt-5\">";
        //htmlcontent += "<div class=\"col-3\">";
        //htmlcontent += "<h5 class=\"almost-gray mb-3\">Invoiced to:</h5>";
        //htmlcontent += $"<p class=\"gray-ish\">{customer.Name}</p>";
        //htmlcontent += $"<p class=\"gray-ish\">{customer.Address}.</p>";
        //htmlcontent += "</div>";
        //htmlcontent += "</div>";
        //htmlcontent += "</div>";
        //htmlcontent += "";

        string htmlcontent = "  <div class=\"container\">\r\n  <div class=\"row invoice-header px-3 py-2\">\r\n    <div class=\"col-4\">\r\n      <p>Company Name</p>\r\n      <h1>INVOICE</h1>\r\n    </div>\r\n    <div class=\"col-4 text-right\">\r\n      <p>(011)-123-1243</p>\r\n      <p>email@adress.com</p>\r\n      <p>personal-website.com</p>\r\n    </div>\r\n    <div class=\"col-4 text-right\">\r\n      <p>Street Adress</p>\r\n      <p>City, State Adress, ZIP</p>\r\n      <p>VAT ID / PID</p>\r\n    </div>\r\n  </div>\r\n\r\n  <div class=\"invoice-content row px-5 pt-5\">\r\n    <div class=\"col-3\">\r\n      <h5 class=\"almost-gray mb-3\">Invoiced to:</h5>\r\n      <p class=\"gray-ish\">Client Name</p>\r\n      <p class=\"gray-ish\">Client Adress spanning on two rows hopefully.</p>\r\n      <p class=\"gray-ish\">VAT ID: 12091803</p>\r\n    </div>\r\n    <div class=\"col-3\">\r\n      <h5 class=\"almost-gray\">Invoice number:</h5>\r\n      <p class=\"gray-ish\"># 123456789</p>\r\n\r\n      <h5 class=\"almost-gray\">Date of Issue:</h5>\r\n      <p class=\"gray-ish\">01 / 01 / 20 20 </p>\r\n\r\n    </div>\r\n    <div class=\"col-6 text-right total-field\">\r\n      <h4 class=\"almost-gray\">Invoice Total</h4>\r\n      <h1 class=\"gray-ish\">634,57 <span class=\"curency\">&euro;</span></h1>\r\n      <h5 class=\"almost-gray due-date\">Due Date: 01 / 01 / 20 20</h5>\r\n    </div>\r\n  </div>\r\n\r\n  <div class=\"row mt-5\">\r\n    <div class=\"col-10 offset-1 invoice-table pt-1\">\r\n      <table class=\"table table-hover\">\r\n            <thead class=\"thead splitForPrint\">\r\n              <tr>\r\n                <th scope=\"col gray-ish\">NO.</th>\r\n                <th scope=\"col gray-ish\">Item</th>\r\n                <th scope=\"col gray-ish\">Qty.</th>\r\n                <th scope=\"col gray-ish\">U. Price</th>\r\n                <th scope=\"col gray-ish\">VAT %</th>\r\n                <th scope=\"col gray-ish\">Discount</th>\r\n                <th class=\"text-right\" scope=\"col gray-ish\">Amount</th>\r\n              </tr>\r\n            </thead>\r\n            <tbody>\r\n              <tr>\r\n                <th scope=\"row\">1</th>\r\n                <td class=\"item\">Item 1</td>\r\n                <td>1</td>\r\n                <td>25 <span class=\"currency\">&euro;</span></td>\r\n                <td>5  %</td>\r\n                <td>5  %</td>\r\n                <td class=\"text-right\">28,75 <span class=\"currency\">&euro;</span></td>\r\n              </tr>\r\n              <tr>\r\n                <th scope=\"row\">2</th>\r\n                <td class=\"item\">Item 2</td>\r\n                <td>1</td>\r\n                <td>25 <span class=\"currency\">&euro;</span> </td>\r\n                <td></td>\r\n                <td>5  %</td>\r\n                <td class=\"text-right\">28,75 <span class=\"currency\">&euro;</span> </td>\r\n              </tr>\r\n              <tr>\r\n                <th scope=\"row\">3</th>\r\n                <td class=\"item\">Item 3</td>\r\n                <td>1</td>\r\n                <td>25 <span class=\"currency\">&euro;</span> </td>\r\n                <td>13 %</td>\r\n                <td>5  %</td>\r\n                <td class=\"text-right\">28,75 <span class=\"currency\">&euro;</span> </td>\r\n              </tr>\r\n              <tr>\r\n                <th scope=\"row\">4</th>\r\n                <td class=\"item\">Item 4</td>\r\n                <td>1</td>\r\n                <td>25 <span class=\"currency\">&euro;</span></td>\r\n                <td>5  %</td>\r\n                <td>5  %</td>\r\n                <td class=\"text-right\">28,75 <span class=\"currency\">&euro;</span></td>\r\n              </tr>\r\n              <tr>\r\n                <th scope=\"row\">5</th>\r\n                <td class=\"item\">Item 5</td>\r\n                <td>1</td>\r\n                <td>25 <span class=\"currency\">&euro;</span> </td>\r\n                <td></td>\r\n                <td>5  %</td>\r\n                <td class=\"text-right\">28,75 <span class=\"currency\">&euro;</span> </td>\r\n              </tr>\r\n              <tr>\r\n                <th scope=\"row\">6</th>\r\n                <td class=\"item\">Item 6</td>\r\n                <td>1</td>\r\n                <td>25 <span class=\"currency\">&euro;</span> </td>\r\n                <td>13 %</td>\r\n                <td>5  %</td>\r\n                <td class=\"text-right\">28,75 <span class=\"currency\">&euro;</span> </td>\r\n              </tr>\r\n              <tr>\r\n                <th scope=\"row\">7</th>\r\n                <td class=\"item\">Item 7</td>\r\n                <td>1</td>\r\n                <td>25 <span class=\"currency\">&euro;</span></td>\r\n                <td>5  %</td>\r\n                <td>5  %</td>\r\n                <td class=\"text-right\">28,75 <span class=\"currency\">&euro;</span></td>\r\n              </tr>\r\n              <tr>\r\n                <th scope=\"row\">8</th>\r\n                <td class=\"item\">Item 8</td>\r\n                <td>1</td>\r\n                <td>25 <span class=\"currency\">&euro;</span> </td>\r\n                <td></td>\r\n                <td>5  %</td>\r\n                <td class=\"text-right\">28,75 <span class=\"currency\">&euro;</span> </td>\r\n              </tr>\r\n              <tr>\r\n                <th scope=\"row\">9</th>\r\n                <td class=\"item\">Item 9</td>\r\n                <td>1</td>\r\n                <td>25 <span class=\"currency\">&euro;</span> </td>\r\n                <td>13 %</td>\r\n                <td>5  %</td>\r\n                <td class=\"text-right\">28,75 <span class=\"currency\">&euro;</span> </td>\r\n              </tr>\r\n              <tr>\r\n                <th scope=\"row\">10</th>\r\n                <td class=\"item\">Item 10</td>\r\n                <td>1</td>\r\n                <td>25 <span class=\"currency\">&euro;</span> </td>\r\n                <td>13 %</td>\r\n                <td>5  %</td>\r\n                <td class=\"text-right\">28,75 <span class=\"currency\">&euro;</span> </td>\r\n              </tr>\r\n              <tr>\r\n                <th scope=\"row\">11</th>\r\n                <td class=\"item\">Item 11</td>\r\n                <td>1</td>\r\n                <td>25 <span class=\"currency\">&euro;</span> </td>\r\n                <td>13 %</td>\r\n                <td>5  %</td>\r\n                <td class=\"text-right\">28,75 <span class=\"currency\">&euro;</span> </td>\r\n              </tr>\r\n              <tr>\r\n                <th scope=\"row\">12</th>\r\n                <td class=\"item\">Item 12</td>\r\n                <td>1</td>\r\n                <td>25 <span class=\"currency\">&euro;</span> </td>\r\n                <td>13 %</td>\r\n                <td>5  %</td>\r\n                <td class=\"text-right\">28,75 <span class=\"currency\">&euro;</span> </td>\r\n              </tr>\r\n              <tr>\r\n                <th scope=\"row\">13</th>\r\n                <td class=\"item\">Item 13</td>\r\n                <td>1</td>\r\n                <td>25 <span class=\"currency\">&euro;</span> </td>\r\n                <td>13 %</td>\r\n                <td>5  %</td>\r\n                <td class=\"text-right\">28,75 <span class=\"currency\">&euro;</span> </td>\r\n              </tr>\r\n              <tr>\r\n                <th scope=\"row\">14</th>\r\n                <td class=\"item\">Item 13</td>\r\n                <td>1</td>\r\n                <td>25 <span class=\"currency\">&euro;</span> </td>\r\n                <td>13 %</td>\r\n                <td>5  %</td>\r\n                <td class=\"text-right\">28,75 <span class=\"currency\">&euro;</span> </td>\r\n              </tr>\r\n              <tr>\r\n                <th scope=\"row\">15</th>\r\n                <td class=\"item\">Item 15</td>\r\n                <td>1</td>\r\n                <td>25 <span class=\"currency\">&euro;</span> </td>\r\n                <td>13 %</td>\r\n                <td>5  %</td>\r\n                <td class=\"text-right\">28,75 <span class=\"currency\">&euro;</span> </td>\r\n              </tr>\r\n              <tr>\r\n                <th scope=\"row\">16</th>\r\n                <td class=\"item\">Item 16</td>\r\n                <td>1</td>\r\n                <td>25 <span class=\"currency\">&euro;</span> </td>\r\n                <td>13 %</td>\r\n                <td>5  %</td>\r\n                <td class=\"text-right\">28,75 <span class=\"currency\">&euro;</span> </td>\r\n              </tr>\r\n              <tr>\r\n                <th scope=\"row\">17</th>\r\n                <td class=\"item\">Item 17</td>\r\n                <td>1</td>\r\n                <td>25 <span class=\"currency\">&euro;</span> </td>\r\n                <td>13 %</td>\r\n                <td>5  %</td>\r\n                <td class=\"text-right\">28,75 <span class=\"currency\">&euro;</span> </td>\r\n              </tr>\r\n              <tr>\r\n                <th scope=\"row\">18</th>\r\n                <td class=\"item\">Item 18</td>\r\n                <td>1</td>\r\n                <td>25 <span class=\"currency\">&euro;</span> </td>\r\n                <td>13 %</td>\r\n                <td>5  %</td>\r\n                <td class=\"text-right\">28,75 <span class=\"currency\">&euro;</span> </td>\r\n              </tr>\r\n              <tr>\r\n                <th scope=\"row\">19</th>\r\n                <td class=\"item\">Item 19</td>\r\n                <td>1</td>\r\n                <td>25 <span class=\"currency\">&euro;</span> </td>\r\n                <td>13 %</td>\r\n                <td>5  %</td>\r\n                <td class=\"text-right\">28,75 <span class=\"currency\">&euro;</span> </td>\r\n              </tr>\r\n              <tr>\r\n                <th scope=\"row\">20</th>\r\n                <td class=\"item\">Item 20</td>\r\n                <td>1</td>\r\n                <td>25 <span class=\"currency\">&euro;</span> </td>\r\n                <td>13 %</td>\r\n                <td>5  %</td>\r\n                <td class=\"text-right\">28,75 <span class=\"currency\">&euro;</span> </td>\r\n              </tr>\r\n              <tr>\r\n                <th scope=\"row\">21</th>\r\n                <td class=\"item\">Item 21</td>\r\n                <td>1</td>\r\n                <td>25 <span class=\"currency\">&euro;</span> </td>\r\n                <td>13 %</td>\r\n                <td>5  %</td>\r\n                <td class=\"text-right\">28,75 <span class=\"currency\">&euro;</span> </td>\r\n              </tr>\r\n              <tr>\r\n                <th scope=\"row\">22</th>\r\n                <td class=\"item\">Item 22</td>\r\n                <td>1</td>\r\n                <td>25 <span class=\"currency\">&euro;</span> </td>\r\n                <td>13 %</td>\r\n                <td>5  %</td>\r\n                <td class=\"text-right\">28,75 <span class=\"currency\">&euro;</span> </td>\r\n              </tr>\r\n              <tr>\r\n                <th scope=\"row\">23</th>\r\n                <td class=\"item\">Item 23</td>\r\n                <td>1</td>\r\n                <td>25 <span class=\"currency\">&euro;</span> </td>\r\n                <td>13 %</td>\r\n                <td>5  %</td>\r\n                <td class=\"text-right\">28,75 <span class=\"currency\">&euro;</span> </td>\r\n              </tr>\r\n            </tbody>\r\n          </table>\r\n    </div>\r\n  </div>\r\n<div class=\"row invoice_details\">\r\n   <!-- invoiced to details -->\r\n   <div class=\"col-4 offset-1 pt-3\">\r\n     <h4 class=\"gray-ish\">Invoice Summary & Notes</h4>\r\n     <p class=\"pt-3 almost-gray\">Lorem ipsum dolor sit amet, consectetur adipiscing elit. Cras purus sapien, ullamcorper quis orci eu, consectetur congue nulla. In a fermentum est, ornare maximus neque. Phasellus metus risus, mattis ac sapien in, volutpat laoreet lectus. Maecenas tincidunt condimentum quam, ut porttitor dui ultricies nec.</p>\r\n   </div>\r\n   <!-- Invoice assets and total -->\r\n        <div class=\"offset-1 col-5 mb-3 pr-4 sub-table\">\r\n          <table class=\"table table-borderless\">\r\n            <tbody>\r\n              <tr>\r\n                <th scope=\"row gray-ish\">Subtotal</th>\r\n                <td class=\"text-right\">75 <span class=\"currency \">&euro;</span></td>\r\n              </tr>\r\n              <tr>\r\n                <th scope=\"row gray-ish\">VAT</th>\r\n                <td class=\"text-right\">11,25 <span class=\"currency\">&euro;</span></td>\r\n              </tr>\r\n              <tr>\r\n                <th scope=\"row gray-ish\">Taxes*</th>\r\n                <td class=\"text-right\">11,25 <span class=\"currency\">&euro;</span></td>\r\n              </tr>\r\n              <tr>\r\n                <th scope=\"row gray-ish\">Discounts</th>\r\n                <td class=\"text-right\">7,5 <span class=\"currency\">&euro;</span></td>\r\n              </tr>\r\n              <tr class=\"last-row\">\r\n                  <th scope=\"row\"><h4>Total</h4></th>\r\n                  <td class=\"text-right\"><h4><span class=\"currency\">&euro;</span> 90,25</h4></td>\r\n              </tr>\r\n            </tbody>\r\n          </table>\r\n        </div>\r\n   </div>\r\n  <p class=\"text-center pb-3\"><em> Taxes will be calculated in &euro; regarding transport and other taxable services.</em></p>\r\n</div>";

        return htmlcontent;
    }
}