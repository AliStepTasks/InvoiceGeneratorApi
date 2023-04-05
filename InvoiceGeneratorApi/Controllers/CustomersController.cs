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
using InvoiceGeneratorApi.Services;
using InvoiceGeneratorApi.Enums;

namespace InvoiceGeneratorApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly InvoiceApiDbContext _context;
        private readonly ICustomerService _customerService;
        public CustomersController(InvoiceApiDbContext context, ICustomerService customerService)
        {
            _context = context;
            _customerService = customerService;
        }

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

            var updatedCustomer = await _customerService.EditCustomer(Email, Name, Address, PhoneNumber, Password, PasswordConfirmation);
            
            return updatedCustomer is not null
                ? updatedCustomer
                : Problem("Something went wrong");
        }

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

        // DELETE: api/Customers/5
        [HttpDelete("{Email}")]
        public async Task<IActionResult> Delete(string Email)
        {
            if (_context.Customers is null)
            {
                return NotFound();
            }

            var isCustomerDeleted = _customerService.DeleteCustomer(Email);

            return isCustomerDeleted is not null
                ? Ok("Customer Deleted")
                : Problem("Something went wrong");
        }
    }
}