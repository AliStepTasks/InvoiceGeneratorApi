using InvoiceGeneratorApi.Data;
using InvoiceGeneratorApi.Models;
using InvoiceGeneratorApi.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace InvoiceGeneratorApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataSeedController : ControllerBase
    {
        private readonly InvoiceApiDbContext _context;

        public DataSeedController(InvoiceApiDbContext context)
        {
            _context = context;
        }


        // GET: api/<DataSeedController>/Customers
        [HttpGet("Generate Customers")]
        public async Task<IEnumerable<Customer>> GenerateCustomers(int numberOfCustomers)
        {
            var customers = SeedDb.CustomerSeed(numberOfCustomers);
            foreach (var cus in customers)
            {
                _context.Customers.Add(cus);
            }
            await _context.SaveChangesAsync();
            return customers;
        }

        // GET api/<DataSeedController>/Invoices
        [HttpGet("Generate Invoices")]
        public async Task<IEnumerable<Invoice>> GenerateInvoices(int numberOfInvoices, int numberOfRows)
        {
            var invoices = SeedDb.InvoiceSeed(numberOfInvoices, numberOfRows);
            foreach (var inv in invoices)
            {
                _context.Invoices.Add(inv);
            }

            await _context.SaveChangesAsync();

            foreach(var inv  in invoices)
            {
                var rows = inv.Rows.Select(r => DtoAndReverseConverter.InvoiceRowDtoToInvoiceRow(r));
                foreach (var row in rows)
                {
                    row.InvoiceId = inv.Id;
                    _context.InvoiceRows.Add(row);
                }
            }

            await _context.SaveChangesAsync();

            return invoices;
        }

        // GET api/<DataSeedController>/Users
        [HttpGet("Generate Users")]
        public async Task<IEnumerable<User>> GenerateUsers(int numberOfUsers)
        {
            var users = SeedDb.UserSeed(numberOfUsers);
            foreach (var user in users)
            {
                _context.Users.Add(user);
            }
            await _context.SaveChangesAsync();
            return users;
        }
    }
}