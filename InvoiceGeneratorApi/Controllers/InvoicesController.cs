using Microsoft.AspNetCore.Mvc;
using InvoiceGeneratorApi.DTO;
using InvoiceGeneratorApi.Data;
using InvoiceGeneratorApi.DTO.Pagination;
using InvoiceGeneratorApi.Enums;
using InvoiceGeneratorApi.Interfaces;
using Syncfusion.HtmlConverter;
using DocumentFormat.OpenXml.Drawing.Charts;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using DocumentFormat.OpenXml.Drawing;
using InvoiceGeneratorApi.Models;
using iTextSharp.text.pdf;
using Rotativa.AspNetCore;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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
        private readonly IHtmlConverterSettings _htmlConverter;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

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
        //[HttpGet("Generate Invoice PDF")]
        //public async Task<IActionResult> GenerateInvoicePDF(int id)
        //{
        //    if (_context.Invoices is null)
        //    {
        //        return NotFound();
        //    }

        //    (MemoryStream response, string contentType, string fileName) = await _invoiceService.GenerateInvoicePDF(id);
        //    return File(response, contentType, fileName);
        ////}


        //[HttpGet("Something")]
        //// Action to generate invoice PDF file
        //public async Task<IActionResult> GenerateInvoicePdf(int invoiceId)
        //{
        //    // Render view to string
        //    var viewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        //    {
        //        Model = new MyViewModel() // Replace with your own view model
        //    };
        //    var htmlContent = await this.ViewToStringAsync("MyView", viewData);

        //    // Convert HTML to PDF
        //    var converter = _htmlConverter.Convert(htmlContent, _tempDataProvider, _serviceProvider);
        //    var pdfData = converter.Save();

        //    // Return PDF file as a file download
        //    return File(pdfData, "application/pdf", "my-pdf-file.pdf");
        //}




    //[HttpGet("Generate Invoice PDF")]
    //public async Task<IActionResult> GenerateInvoicePDF(int id)
    //{
    //    var invoice = await _context.Invoices.FindAsync(id);
    //    var customer = await _context.Customers.FindAsync(invoice.CustomerId);

    //    // Load the HTML template for the invoice
    //    var html = System.IO.File.ReadAllText("invoice.html");

    //    // Replace placeholders in the HTML template with actual invoice data
    //    html = html.Replace("{CustomerName}", customer.Name);
    //    html = html.Replace("{InvoiceNumber}", Guid.NewGuid().ToString());
    //    // ...

    //    // Create a new PDF document
    //    var document = new Document(PageSize.A4, 50, 50, 25, 25);

    //    // Create a new MemoryStream to store the PDF document
    //    var stream = new MemoryStream();

    //    // Create a new PdfWriter to write the PDF document to the MemoryStream
    //    var writer = PdfWriter.GetInstance(document, stream);

    //    // Close the PDF document
    //    document.Close();

    //    // Set the content type and filename for the PDF document
    //    var contentType = "application/pdf";
    //    var fileName = "invoice.pdf";

    //    // Write the PDF document to a file stream
    //    var fileStream = new FileStreamResult(new MemoryStream(stream.ToArray()), contentType);

    //    // Set the file download name
    //    fileStream.FileDownloadName = fileName;

    //    // Return the file stream as the result of the action
    //    return fileStream;
    //}
}
}