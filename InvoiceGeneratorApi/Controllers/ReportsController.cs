using InvoiceGeneratorApi.Data;
using InvoiceGeneratorApi.Interfaces;
using InvoiceGeneratorApi.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace InvoiceGeneratorApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly InvoiceApiDbContext _context;
        private readonly IReportService _reportService;

        public ReportsController(InvoiceApiDbContext context, IReportService reportService)
        {
            _context = context;
            _reportService = reportService;
        }

        // GET api/<ReportsController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> DownloadCustomerReport()
        {
            // Get the list of customers from the database
            var customers = _context.Customers
                .Select(c => DtoAndReverseConverter.CustomerToCustomerDto(c))
                .ToList();

            // Generate the customer report
            var report = await _reportService.GenerateCustomerReport(customers);

            // Return the report as a downloadable Excel file
            return File(report,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "customer_" + Guid.NewGuid().ToString() + "_report.xlsx");
        }
    }
}