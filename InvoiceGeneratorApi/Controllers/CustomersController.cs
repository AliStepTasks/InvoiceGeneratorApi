using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InvoiceGeneratorApi.Data;
using InvoiceGeneratorApi.Models;
using InvoiceGeneratorApi.DTO;
using InvoiceGeneratorApi.DTO.Pagination;
using InvoiceGeneratorApi.Enums;
using InvoiceGeneratorApi.Interfaces;

namespace InvoiceGeneratorApi.Controllers
{
    /// <summary>
    /// The controller responsible for handling customer-related requests.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly InvoiceApiDbContext _context;
        private readonly ICustomerService _customerService;

        /// <summary>
        /// Initializes a new instance of the CustomersController class with the specified database context and customer service.
        /// </summary>
        /// <param name="context">The database context to use for accessing customer data.</param>
        /// <param name="customerService">The customer service to use for performing customer-related operations.</param>
        public CustomersController(InvoiceApiDbContext context, ICustomerService customerService)
        {
            _context = context;
            _customerService = customerService;
        }

        /// <summary>
        /// Retrieves a paginated list of customers based on the provided pagination and search parameters.
        /// </summary>
        /// <param name="request">The pagination request object containing the desired page number and page size of the returned result.</param>
        /// <param name="search">The search string used to filter the returned list of customers.</param>
        /// <param name="orderBy">The object used to specify the order in which the customers are sorted.</param>
        /// <returns>A paginated list of CustomerDTO objects.</returns>
        // GET: api/Customers
        [HttpGet]
        public async Task<ActionResult<PaginationDTO<CustomerDTO>>> Gets(
            [FromQuery] PaginationRequest request, string? search, OrderBy? orderBy)
        {
            if (_context.Customers is null)
            {
                return NotFound();
            }

            var paginatedList = await _customerService.GetCustomers(request.Page, request.PageSize, search, orderBy);

            return paginatedList is not null
                ? paginatedList
                : Problem("Something went wrong");
        }

        /// <summary>
        /// Retrieves a single customer based on their email address.
        /// </summary>
        /// <param name="Email">The email address of the customer to retrieve.</param>
        /// <returns>A CustomerDTO object representing the retrieved customer, or a Problem HTTP response if something went wrong.</returns>
        // GET: api/Customers/5
        [HttpGet("{Email}")]
        public async Task<ActionResult<CustomerDTO>> Get(string Email)
        {
            if (_context.Customers is null)
            {
                return NotFound();
            }

            var customerDto = await _customerService.GetCustomer(Email);

            return customerDto is not null
                ? customerDto
                : Problem("Something went wrong");
        }

        /// <summary>
        /// Updates a single customer's information based on their email.
        /// </summary>
        /// <param name="Email">The email address of the customer to update.</param>
        /// <param name="Name">The updated name of the customer.</param>
        /// <param name="Address">The updated address of the customer.</param>
        /// <param name="PhoneNumber">The updated phone number of the customer.</param>
        /// <param name="Password">The updated password of the customer.</param>
        /// <param name="PasswordConfirmation">The confirmation of the updated password of the customer.</param>
        /// <returns>A CustomerDTO object representing the updated customer, or a Problem HTTP response if something went wrong.</returns>

        // PUT: api/Customers/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("Email, Name, Address, PhoneNumber, Password, PasswordConfirmation")]
        public async Task<ActionResult<CustomerDTO>> Put(
            string Email, string? Name, string? Address,
            string? PhoneNumber, string Password, string PasswordConfirmation)
        {
            if (Email is null || Password is null || PasswordConfirmation is null)
            {
                return BadRequest();
            }
            var updatedCustomer = await _customerService.EditCustomer(Email, Name, Address, PhoneNumber, Password);
            
            return updatedCustomer is not null
                ? updatedCustomer
                : Problem("Something went wrong");
        }

        /// <summary>
        /// Changes the status of a single customer based on their email.
        /// </summary>
        /// <param name="Email">The email address of the customer whose status to change.</param>
        /// <param name="Status">The new status to set for the customer.</param>
        /// <returns>A CustomerDTO object representing the updated customer, or a Problem HTTP response if something went wrong.</returns>

        [HttpPost("Email, Status")]
        public async Task<ActionResult<CustomerDTO>> Change(string Email, CustomerStatus Status)
        {
            if (Email is null)
            {
                return BadRequest();
            }

            var customerDTO = await _customerService.ChangeCustomerStatus(Email, Status);

            return customerDTO is not null 
                ? customerDTO 
                : Problem("Something went wrong");
        }

        /// <summary>
        /// Adds a new customer to the database.
        /// </summary>
        /// <param name="customerDTO">The CustomerDTO object representing the new customer to add.</param>
        /// <returns>A CustomerDTO object representing the newly added customer, or a Problem HTTP response if something went wrong.</returns>
        // POST: api/Customers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("CustomerDTO")]
        public async Task<ActionResult<CustomerDTO>> Post(CustomerDTO customerDTO)
        {
            if (customerDTO is null)
            {
                return Problem("Entity set 'InvoiceApiDbContext.Customers'  is null.");
            }

            customerDTO = await _customerService.AddCustomer(customerDTO);

            return customerDTO is not null
                ? customerDTO
                : Problem("Something went wrong");
        }

        /// <summary>
        /// Deletes a customer from the database based on their email.
        /// </summary>
        /// <param name="Email">The email address of the customer to delete.</param>
        /// <returns>An Ok HTTP response if the customer was successfully deleted, or a Problem HTTP response if something went wrong.</returns>
        // DELETE: api/Customers/5
        [HttpDelete("{Email}")]
        public async Task<IActionResult> Delete(string Email)
        {
            if (_context.Customers is null)
            {
                return NotFound();
            }

            var isCustomerDeleted = await _customerService.DeleteCustomer(Email);

            return isCustomerDeleted is not null
                ? Ok("Customer Deleted")
                : Problem("Something went wrong");
        }
    }
}