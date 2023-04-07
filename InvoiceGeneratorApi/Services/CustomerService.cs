using InvoiceGeneratorApi.Data;
using InvoiceGeneratorApi.DTO;
using InvoiceGeneratorApi.DTO.Pagination;
using InvoiceGeneratorApi.Enums;
using InvoiceGeneratorApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvoiceGeneratorApi.Services;

public class CustomerService : ICustomerService
{
    private readonly InvoiceApiDbContext _context;

    public CustomerService(InvoiceApiDbContext context)
    {
        _context = context;
    }
    public async Task<CustomerDTO> AddCustomer(CustomerDTO customerDTO)
    {
        var customer = DtoAndReverseConverter.CustomerDtoToCustomer(customerDTO);

        customer.CreatedAt = DateTimeOffset.Now;
        customer.UpdatedAt = DateTimeOffset.Now;
        customer.DeletedAt = DateTimeOffset.MinValue;

        customer.Password = BCrypt.Net.BCrypt.HashPassword(customer.Password);

        customer = _context.Customers.Add(customer).Entity;
        await _context.SaveChangesAsync();

        return DtoAndReverseConverter.CustomerToCustomerDto(customer);
    }

    /// <summary>
    /// Find customer by Email and then change the status
    /// </summary>
    /// <param name="Email"></param>
    /// <param name="Status"></param>
    /// <returns></returns>
    public async Task<CustomerDTO> ChangeCustomerStatus(string Email, CustomerStatus Status)
    {
        var customer = _context.Customers.FirstOrDefault(c => c.Email == Email);

        if (customer is null)
        {
            return null;
        }

        customer.Status = Status;
        customer = _context.Customers.Update(customer).Entity;
        await _context.SaveChangesAsync();

        return DtoAndReverseConverter.CustomerToCustomerDto(customer);
    }

    /// <summary>
    /// Delete customer according to Email
    /// </summary>
    /// <param name="Email"></param>
    /// <returns></returns>
    public async Task<object> DeleteCustomer(string Email)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == Email);

        if(customer is null)
        {
            return null;
        }

        bool isThereValidInvoices = _context.Invoices.Count(i => i.CustomerId == customer.Id) > 0;

        if (isThereValidInvoices)
        {
            return null;
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// Edit customer data with password confirmation
    /// </summary>
    /// <param name="Email"></param>
    /// <param name="Name"></param>
    /// <param name="Address"></param>
    /// <param name="PhoneNumber"></param>
    /// <param name="Password"></param>
    /// <param name="PasswordConfirmation"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task<CustomerDTO> EditCustomer(
        string Email, string? Name,
        string? Address, string? PhoneNumber, string Password)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync
            (c => c.Email == Email);

        var isValidPassword = BCrypt.Net.BCrypt.Verify(Password, customer.Password);

        if (isValidPassword)
        {
            return null;
        }

        customer.Name = Name is not null ? Name : customer.Name;
        customer.Address = Address is not null ? Address : customer.Address;
        customer.PhoneNumber = PhoneNumber is not null ? PhoneNumber : customer.PhoneNumber;
        customer.UpdatedAt = DateTimeOffset.UtcNow;

        customer = _context.Customers.Update(customer).Entity;
        await _context.SaveChangesAsync();

        return DtoAndReverseConverter.CustomerToCustomerDto(customer);
    }

    /// <summary>
    /// Get customer according to Email
    /// </summary>
    /// <param name="Email"></param>
    /// <returns></returns>
    public async Task<CustomerDTO> GetCustomer(string Email)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == Email);

        if (customer is null)
        {
            return null;
        }

        return DtoAndReverseConverter.CustomerToCustomerDto(customer);
    }

    /// <summary>
    /// Get all customers
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public async Task<PaginationDTO<CustomerDTO>> GetCustomers(int page, int pageSize, string? search, OrderBy? orderBy)
    {
        IQueryable<Customer> query = _context.Customers;

        // Search
        if(!string.IsNullOrEmpty(search))
        {
            query = query.Where(c => c.Name.Contains(search));
        }

        // Sorting
        if(OrderBy.Ascending == orderBy)
        {
            query = query.OrderBy(c => _context.Invoices.Count(i => i.CustomerId == c.Id));
        }
        else if (OrderBy.Descending == orderBy)
        {
            query = query.OrderByDescending(c => _context.Invoices.Count(i => i.CustomerId == c.Id));
        }

        // Pagination

        var customerList = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

        var customerDtoList = customerList.Select(c => DtoAndReverseConverter.CustomerToCustomerDto(c));

        var paginatedList = new PaginationDTO<CustomerDTO>
        (
            customerDtoList,
            new PaginatinoMeta(page, pageSize, _context.Customers.Count())
        );

        return paginatedList;
    }
}