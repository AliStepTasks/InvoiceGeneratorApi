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
        public async Task<ActionResult<PaginationDTO<InvoiceDTO>>> GetInvoiceDTO(
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
        public async Task<ActionResult<InvoiceDTO>> GetInvoiceDTO(int id)
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

        [HttpGet("Generate Invoice PDF")]
        public async Task<IActionResult> GenerateInvoicePDF(int id)
        {
            if (_context.Invoices is null)
            {
                return NotFound();
            }

            (byte[] response, string contentType, string fileName) = await _invoiceService.GenerateInvoicePDF(id);
            return File(response, contentType, fileName);
        }

        // PUT: api/InvoiceDTOes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInvoiceDTO(int id, InvoiceDTO invoiceDTO)
        {
            if (id != invoiceDTO.Id)
            {
                return BadRequest();
            }

            _context.Entry(invoiceDTO).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvoiceDTOExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/InvoiceDTOes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<InvoiceDTO>> PostInvoiceDTO(InvoiceDTO invoiceDTO)
        {
            if (_context.Invoices == null)
            {
                return Problem("Entity set 'InvoiceApiDbContext.InvoiceDTO'  is null.");
            }

            return CreatedAtAction("GetInvoiceDTO", new { id = invoiceDTO.Id }, invoiceDTO);
        }

        // DELETE: api/InvoiceDTOes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoiceDTO(int id)
        {
            if (_context.Invoices == null)
            {
                return NotFound();
            }
            var invoiceDTO = await _context.Invoices.FindAsync(id);
            if (invoiceDTO == null)
            {
                return NotFound();
            }

            _context.Invoices.Remove(invoiceDTO);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool InvoiceDTOExists(int id)
        {
            return (_context.Invoices?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}