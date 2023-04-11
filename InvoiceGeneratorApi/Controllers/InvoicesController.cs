using Microsoft.AspNetCore.Mvc;
using InvoiceGeneratorApi.DTO;
using InvoiceGeneratorApi.Data;
using InvoiceGeneratorApi.DTO.Pagination;
using InvoiceGeneratorApi.Enums;
using InvoiceGeneratorApi.Interfaces;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Authorization;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using InvoiceGeneratorApi.PdfRelated;
using System.Diagnostics;
using InvoiceGeneratorApi.PdfRelated.Models;
using InvoiceGeneratorApi.Services;
using Bogus;
using Xceed.Words.NET;
using Microsoft.EntityFrameworkCore;

namespace InvoiceGeneratorApi.Controllers
{
    /// <summary>
    /// Controller for managing invoices.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly InvoiceApiDbContext _context;
        private readonly IServiceInvoice _invoiceService;

        /// <summary>
        /// Initializes a new instance of the <see cref="InvoicesController"/> class with the specified <paramref name="context"/> and <paramref name="serviceInvoice"/> parameters.
        /// </summary>
        /// <param name="context">The context used to interact with the invoice database.</param>
        /// <param name="serviceInvoice">The service used to perform operations on invoice data.</param>
        public InvoicesController(InvoiceApiDbContext context, IServiceInvoice serviceInvoice)
        {
            _context = context;
            _invoiceService = serviceInvoice;

        }

        /// <summary>
        /// Retrieves a paginated list of invoices based on the provided search and order by parameters.
        /// </summary>
        /// <param name="request">The pagination request object containing the desired page and page size.</param>
        /// <param name="search">The search term to filter invoices by.</param>
        /// <param name="orderBy">The order by criteria to sort invoices by.</param>
        /// <returns>The paginated list of invoices.</returns>
        // GET: api/InvoiceDTOes
        [HttpGet]
        public async Task<ActionResult<PaginationDTO<InvoiceDTO>>> GetInvoices(
            [FromQuery] PaginationRequest request, string? search, OrderBy? orderBy)
        {
            if (_context.Invoices is null)
            {
                return NotFound();
            }

            var paginatedList = await _invoiceService.GetInvoices(
                request.Page, request.PageSize,
                search, orderBy);

            return paginatedList is not null
                ? paginatedList
                : Problem("Something went wrong");
        }

        /// <summary>
        /// Gets an invoice by ID.
        /// </summary>
        /// <param name="id">The ID of the invoice to get.</param>
        /// <returns>The requested invoice.</returns>
        // GET: api/InvoiceDTOes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<InvoiceDTO>> GetInvoice(int id)
        {
            if (_context.Invoices is null)
            {
                return NotFound();
            }

            var invoiceDto = await _invoiceService.GetInvoice(id);

            return invoiceDto is not null
                ? invoiceDto
                : Problem("Something went wrong");
        }

        /// <summary>
        /// Creates a new invoice.
        /// </summary>
        /// <param name="invoiceDTO">The invoice data to create the invoice from.</param>
        /// <returns>The newly created invoice.</returns>
        // POST: api/InvoiceDTOes
        [HttpPost]
        public async Task<ActionResult<InvoiceDTO>> PostInvoice(InvoiceDTO invoiceDTO)
        {
            if (_context.Invoices == null)
            {
                return BadRequest();
            }

            var invoice = await _invoiceService.CreateInvoice(invoiceDTO);

            return invoice is not null
                ? invoice
                : Problem("Something went wrong");
        }

        /// <summary>
        /// Updates an existing invoice.
        /// </summary>
        /// <param name="invoiceId">The ID of the invoice to update.</param>
        /// <param name="customerId">The new customer ID for the invoice.</param>
        /// <param name="startDate">The new start date for the invoice.</param>
        /// <param name="endDate">The new end date for the invoice.</param>
        /// <param name="comment">The new comment for the invoice.</param>
        /// <param name="status">The new status for the invoice.</param>
        /// <returns>The updated invoice.</returns>
        // PUT: api/InvoiceDTOes/5
        [HttpPut("invoiceId, customerId, startDate, endDate, comment, status")]
        public async Task<ActionResult<InvoiceDTO>> PutInvoice(
        int invoiceId, int? customerId, DateTimeOffset? startDate,
        DateTimeOffset? endDate, string? comment, InvoiceStatus? status)
        {
            var invoice = await _invoiceService.EditInvoice(
                invoiceId, customerId, startDate,
                endDate, comment, status);

            return invoice is not null
                ? invoice
                : Problem("Something went wrong");
        }

        /// <summary>
        /// Changes the status of an invoice.
        /// </summary>
        /// <param name="id">The ID of the invoice to change the status of.</param>
        /// <param name="status">The new status for the invoice.</param>
        /// <returns>The updated invoice.</returns>
        // PUT: api/InvoiceDTOes/5
        [HttpPut("status")]
        public async Task<ActionResult<InvoiceDTO>> PutInvoiceStatus(int id, InvoiceStatus status)
        {
            var invoice = await _invoiceService.ChangeInvoiceStatus(id, status);

            return invoice is not null
                ? invoice
                : Problem("Something went wrong");
        }

        /// <summary>
        /// Deletes an invoice.
        /// </summary>
        /// <param name="id">The ID of the invoice to delete.</param>
        /// <returns>The deleted invoice.</returns>
        // DELETE: api/InvoiceDTOes/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<InvoiceDTO>> DeleteInvoice(int id)
        {
            if (_context.Invoices == null)
            {
                return NotFound();
            }

            var deletedInvoice = await _invoiceService.DeleteInvoice(id);

            return deletedInvoice is not null
                ? deletedInvoice
                : Problem("Something went wrong");
        }

        /// <summary>
        /// Generates a PDF of an invoice.
        /// </summary>
        /// <param name="id">The ID of the invoice to generate a PDF for.</param>
        /// <returns>A file containing the generated PDF.</returns>
        [HttpGet("Generate Invoice PDF")]
        public async Task<IActionResult> GenerateInvoicePDF(int invoiceId)
        {
            var fileBytes = await _invoiceService.GenerateInvoicePDF(invoiceId);

            return File(fileBytes, "application/pdf", "invoice.pdf");
        }

        [HttpGet("Generate Invoice DocX")]
        public async Task<IActionResult> GenerateInvoiceDocX(int invoiceId)
        {

            var fileBytes = await _invoiceService.GenerateInvoiceDocX(invoiceId);

            // Return the File
            return fileBytes is not null
                ? File(fileBytes, "application/docx", "invoice.docx")
                : Problem("Something went wrong");
        }
    }
}