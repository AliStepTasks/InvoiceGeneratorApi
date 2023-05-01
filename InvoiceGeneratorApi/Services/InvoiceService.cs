using Bogus;
using Bogus.DataSets;
using InvoiceGeneratorApi.Data;
using InvoiceGeneratorApi.DTO;
using InvoiceGeneratorApi.DTO.Pagination;
using InvoiceGeneratorApi.Enums;
using InvoiceGeneratorApi.Interfaces;
using InvoiceGeneratorApi.Models;
using InvoiceGeneratorApi.PdfRelated.Models;
using InvoiceGeneratorApi.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Utilities;
using QuestPDF.Fluent;
using Serilog;
using System;
using Xceed.Words.NET;

namespace InvoiceGeneratorApi.Services;

public class InvoiceService : IServiceInvoice
{
    private readonly InvoiceApiDbContext _context;
    private UserInfo _userInfo;

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
            return null!;

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
    /// <param name="invoiceId"></param>
    /// <returns></returns>
    public async Task<InvoiceDTO> DeleteInvoice(int invoiceId)
    {
        var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == invoiceId);

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
            Log.Information($"There is no invoice with this id -> {invoiceId}");
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
    /// <param name="invoiceId"></param>
    /// <returns></returns>
    public async Task<InvoiceDTO> GetInvoice(int invoiceId)
    {
        var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice is null)
        {
            Log.Information($"There is no invoice with this id -> {invoiceId}");
            return null!;
        }

        var isThisInvoiceOfUser = _context.UserCustomerRelation
            .Any(u => u.UserId == _userInfo.UserId && u.CustomerId == invoice.CustomerId);

        if (!isThisInvoiceOfUser)
        {
            Log.Information($"Invoice with this ID -> {invoiceId} is not belong to {_userInfo.UserName}." +
                $"Please check the credentials.");
            return null;
        }

        var invoiceRowsDto = _context.InvoiceRows
            .Where(r => r.InvoiceId == invoice.Id)
            .Select(r => DtoAndReverseConverter.InvoiceRowToInvoiceRowDto(r));
        invoice.Rows = invoiceRowsDto.ToArray();

        return DtoAndReverseConverter.InvoiceToInvoiceDto(invoice);
    }

    public async Task SetUserInfo(UserInfo userInfo)
    {
        _userInfo = userInfo;
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
        var customersOfUser = _context.UserCustomerRelation
            .Where(u => u.UserId == _userInfo.UserId)
            .Select(u => u.CustomerId);


        IQueryable<Invoice> query = _context.Invoices.Where(i => customersOfUser.Contains(i.CustomerId));

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

    public async Task<byte[]> GenerateInvoicePDF(int invoiceId)
    {
        var invoice = await _context.Invoices.FindAsync(invoiceId);
        if(invoice is null)
        {
            Log.Information($"There is no invoice with this id -> {invoiceId}");
            return null;
        }

        invoice.Rows = _context.InvoiceRows
            .Where(x => x.InvoiceId == invoice.Id)
            .Select(x => DtoAndReverseConverter.InvoiceRowToInvoiceRowDto(x))
            .ToArray();

        var customer = new Customer();
        var invoiceCustomer = await _context.Customers.FindAsync(invoice.CustomerId);
        if (invoiceCustomer is null)
        {
            customer.Name = "Unknown Customer";
            customer.Address = "Unknown Adress";
            customer.Email = "Unknown Email";
            customer.PhoneNumber = "Unknown Number";
        }
        else
            customer = invoiceCustomer;

        var faker = new Faker();
        var model = new InvoiceModel
        {
            InvoiceDto = DtoAndReverseConverter.InvoiceToInvoiceDto(invoice),
            InvoiceNumber = invoice.Id.ToString(),
            DueDate = invoice.CreatedAt.AddDays(14).DateTime,
            SellerAddress = new PdfRelated.Models.Address
            {
                CompanyName = "Schofrie LLC",
                Street = faker.Address.StreetAddress(),
                City = faker.Address.City(),
                State = faker.Address.State(),
                Email = "aguliyev45@gmail.com",
                Phone = "+994 (050) 499 97 44"
            },
            CustomerAddress = new PdfRelated.Models.Address
            {
                CompanyName = null,
                Street = customer.Address,
                City = faker.Address.City(),
                State = faker.Address.State(),
                Email = customer.Email,
                Phone = customer.PhoneNumber
            },
        };

        var document = new InvoiceDocument(model);

        var byteFile = document.GeneratePdf();

        return byteFile;
    }

    public async Task<byte[]> GenerateInvoiceDocX(int invoiceId)
    {
        var invoice = await _context.Invoices.FindAsync(invoiceId);
        if (invoice is null)
        {
            Log.Information($"There is no invoice with this id -> {invoiceId}");
            return null;
        }

        invoice.Rows = _context.InvoiceRows
            .Where(x => x.InvoiceId == invoice.Id)
            .Select(x => DtoAndReverseConverter.InvoiceRowToInvoiceRowDto(x))
            .ToArray();

        var customer = await _context.Customers.FindAsync(invoice.CustomerId);

        // Create a new document
        DocX document = DocX.Create("Invoice.docx");

        // Add content to the document
        document.InsertParagraph().AppendLine("Invoice Details").Bold().FontSize(16);
        document.InsertParagraph().AppendLine($"Invoice Number: {invoice.Id}");
        document.InsertParagraph().AppendLine($"Customer Name: {customer.Name}");
        document.InsertParagraph().AppendLine($"Start Date: {invoice.StartDate}");
        document.InsertParagraph().AppendLine($"End Date: {invoice.EndDate}");
        document.InsertParagraph().AppendLine("Invoice Rows:");
        if (invoice.Rows != null)
        {
            foreach (var row in invoice.Rows)
            {
                document.InsertParagraph().AppendLine($"- Service: {row.Service}, Quantity: {row.Quantity}, Unit Price: {row.Amount}$, Total: {row.Sum}$");
            }
        }
        document.InsertParagraph().AppendLine($"Total Sum: {invoice.TotalSum}$");
        if (!string.IsNullOrEmpty(invoice.Comment))
        {
            document.InsertParagraph().AppendLine($"Comment: {invoice.Comment}");
        }
        document.InsertParagraph().AppendLine($"Status: {invoice.Status}");
        document.InsertParagraph().AppendLine($"Created At: {invoice.CreatedAt}");

        // Save the document to a MemoryStream
        MemoryStream stream = new MemoryStream();
        document.SaveAs(stream);
        stream.Position = 0;

        // Convert the MemoryStream to a byte array
        byte[] fileBytes = stream.ToArray();

        // Close the document
        document.Dispose();

        return fileBytes;
    }
}