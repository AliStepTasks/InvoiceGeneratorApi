using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InvoiceGeneratorApi.DTO;
using InvoiceGeneratorApi.Data;
using InvoiceGeneratorApi.Services;
using InvoiceGeneratorApi.DTO.Pagination;
using InvoiceGeneratorApi.Enums;
using InvoiceGeneratorApi.Models;

namespace InvoiceGeneratorApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly InvoiceApiDbContext _context;

        private readonly IServiceInvoice _invoiceService;

        public InvoicesController(InvoiceApiDbContext context, IServiceInvoice serviceInvoice)
        {
            _context = context;
            _invoiceService = serviceInvoice;

        }

        // GET: api/InvoiceDTOes
        [HttpGet]
        public async Task<ActionResult<PaginationDTO<InvoiceDTO>>> GetInvoice(
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

        // PUT: api/InvoiceDTOes/5
        [HttpPut("invoiceId, customerId, startDate, endDate, comment, status")]
        public async Task<ActionResult<InvoiceDTO>> PutInvoice(
        int invoiceId, int?  customerId, DateTimeOffset? startDate,
        DateTimeOffset? endDate, string? comment, InvoiceStatus? status)
        {
            var invoice = await _invoiceService.EditInvoice(
                invoiceId, customerId, startDate,
                endDate, comment, status);

            return invoice is not null
                ? invoice
                : Problem("Something went wrong");
        }

        // PUT: api/InvoiceDTOes/5
        [HttpPut("status")]
        public async Task<ActionResult<InvoiceDTO>> PutInvoiceStatus(int id, InvoiceStatus status)
        {
            var invoice = await _invoiceService.ChangeInvoiceStatus(id, status);

            return invoice is not null
                ? invoice
                : Problem("Something went wrong");
        }

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

        [HttpGet("Generate Invoice PDF")]
        public async Task<IActionResult> GenerateInvoicePDF(int id)
        {
            if (_context.Invoices is null)
            {
                return NotFound();
            }

            (MemoryStream response, string contentType, string fileName) = await _invoiceService.GenerateInvoicePDF(id);
            return File(response, contentType, fileName);
        }
    }
}