using InvoiceGeneratorApi.Data;
using InvoiceGeneratorApi.Interfaces;
using InvoiceGeneratorApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace InvoiceGeneratorApi.Controllers
{
    /// <summary>
    /// Controller for generating reports.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly InvoiceApiDbContext _context;
        private readonly IReportService _reportService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReportsController"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="reportService">The report service.</param>
        public ReportsController(InvoiceApiDbContext context, IReportService reportService)
        {
            _context = context;
            _reportService = reportService;
        }

        /// <summary>
        /// Downloads a customer report as an Excel file.
        /// </summary>
        /// <returns>The generated customer report as a downloadable Excel file.</returns>
        [HttpGet("Customer Report")]
        public async Task<IActionResult> DownloadCustomerReport()
        {
            // Get the list of customers from the database
            var customers = _context.Customers
                .Select(c => DtoAndReverseConverter.CustomerToCustomerDto(c))
                .ToList();
            var invoices = _context.Invoices.ToList();

            // Generate the customer report
            var report = await _reportService.GenerateCustomerReport(customers, invoices);

            // Return the report as a downloadable Excel file
            return File(report,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "customer_report.xlsx");
        }

        /// <summary>
        /// Downloads a report of work done as an Excel file.
        /// </summary>
        /// <returns>The generated report of work done as a downloadable Excel file.</returns>
        [HttpGet("Work Done Report")]
        public async Task<IActionResult> DownloadDoneWorkReport()
        {
            // Get the list of invoices from the database
            var invoices = _context.Invoices
                .Select(i => DtoAndReverseConverter.InvoiceToInvoiceDto(i))
                .ToList();
            var customers = _context.Customers.ToList();

            // Generate the customer report
            var report = await _reportService.GenerateDoneWorkReport(invoices, customers);

            // Return the report as a downloadable Excel file
            return File(report,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "doneWork_report.xlsx");
        }

        /// <summary>
        /// Downloads an invoice report as an Excel file.
        /// </summary>
        /// <returns>The generated invoice report as a downloadable Excel file.</returns>
        [HttpGet("Invoice Report")]
        public async Task<IActionResult> DownloadInvoiceReport()
        {
            // Get the list of invoices from the database
            var invoices = _context.Invoices
                .Select(i => DtoAndReverseConverter.InvoiceToInvoiceDto(i))
                .ToList();
            var customers = _context.Customers.ToList();

            // Generate the customer report
            var report = await _reportService.GenerateInvoiceReport(invoices, customers);

            // Return the report as a downloadable Excel file
            return File(report,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "invoice_report.xlsx");
        }
    }
}