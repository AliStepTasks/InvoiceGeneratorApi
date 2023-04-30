using Bogus;
using InvoiceGeneratorApi.Data;
using InvoiceGeneratorApi.Models;
using InvoiceGeneratorApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace InvoiceGeneratorApi.Controllers
{
    /// <summary>
    /// Controller responsible for seeding the database with test data. Provides methods for generating customers, invoices, and users.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DataSeedController : ControllerBase
    {
        private readonly InvoiceApiDbContext _context;

        /// <summary>
        /// Initializes a new instance of the DataSeedController class with the specified database context.
        /// </summary>
        /// <param name="context">The database context to use for seeding data.</param>
        public DataSeedController(InvoiceApiDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Generates the specified number of customers and adds them to the database.
        /// </summary>
        /// <param name="numberOfCustomers">The number of customers to generate.</param>
        /// <returns>An IEnumerable of the generated customers.</returns>
        // GET: api/<DataSeedController>/Customers
        [HttpGet("Generate Customers")]
        public async Task<IEnumerable<Customer>> GenerateCustomers(int numberOfCustomers)
        {
            if(_context.Users is null)
            {
                Log.Information("No users found in database. Please seed users first.");
                return null;
            }

            var customers = SeedDb.CustomerSeed(numberOfCustomers);
            var faker = new Faker();
            foreach (var customer in customers)
            {
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
                var userCustomerRelation = new UserCustomerRelation
                {
                    CustomerId = customer.Id,
                    UserId = faker.PickRandom(_context.Users.Select(u => u.Id).ToArray())
                };
                _context.UserCustomerRelation.Add(userCustomerRelation);
            }

            await _context.SaveChangesAsync();
            Log.Information($"{numberOfCustomers} customers are generated and added to the database.");

            return customers;
        }

        /// <summary>
        /// Generates the specified number of invoices with the specified number of rows per invoice, and saves them to the database.
        /// </summary>
        /// <param name="numberOfInvoices">The number of invoices to generate.</param>
        /// <param name="numberOfRows">The number of rows per invoice to generate.</param>
        /// <returns>The generated invoices.</returns>
        // GET api/<DataSeedController>/Invoices
        [HttpGet("Generate Invoices")]
        public async Task<IEnumerable<Invoice>> GenerateInvoices(int numberOfInvoices, int numberOfRows)
        {
            int[] customersId = _context.Customers.Select(c => c.Id).ToArray();
            var invoices = SeedDb.InvoiceSeed(numberOfInvoices, numberOfRows, customersId);
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
            Log.Information($"{numberOfInvoices} invoices with range of (1, {numberOfRows}) rows are generated and added to the database.");

            return invoices;
        }

        /// <summary>
        /// Generates a specified number of new users and adds them to the database.
        /// </summary>
        /// <param name="numberOfUsers">The number of users to generate.</param>
        /// <returns>A list of the newly generated users.</returns>
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
            Log.Information($"{numberOfUsers} users are generated and added to the database.");

            return users;
        }
    }
}