using DocumentFormat.OpenXml.Presentation;
using InvoiceGeneratorApi.Data;
using InvoiceGeneratorApi.DTO;
using InvoiceGeneratorApi.DTO.Pagination;
using InvoiceGeneratorApi.Enums;
using InvoiceGeneratorApi.Interfaces;
using InvoiceGeneratorApi.Models;
using InvoiceGeneratorApi.Providers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace InvoiceGeneratorApi.Services;

public class CustomerService : ICustomerService
{
    private readonly InvoiceApiDbContext _context;
    private readonly IMemoryCache _memoryCache;
    private UserInfo _userInfo;

    public CustomerService(InvoiceApiDbContext context, IMemoryCache memoryCache)
    {
        _context = context;
        _memoryCache = memoryCache;
    }
    public async Task<CustomerDTO> AddCustomer(CustomerDTO customerDTO)
    {
        var customer = DtoAndReverseConverter.CustomerDtoToCustomer(customerDTO);

        customer.CreatedAt = DateTimeOffset.Now;
        customer.UpdatedAt = DateTimeOffset.Now;
        customer.DeletedAt = DateTimeOffset.MinValue;

        customer.Password = BCrypt.Net.BCrypt.HashPassword(customer.Password);

        customer = _context.Customers.Add(customer).Entity;
        _context.UserCustomerRelation.Add(new UserCustomerRelation
        {
            UserId = _userInfo.UserId,
            CustomerId = customer.Id
        });
        await _context.SaveChangesAsync();
        Log.Information($"Customer {customerDTO.Email} added to database.");

        _memoryCache.Set(customer.Email, customer, TimeSpan.FromMinutes(10));
        return DtoAndReverseConverter.CustomerToCustomerDto(customer);
    }

    public async Task<CustomerDTO> ChangeCustomerStatus(string Email, CustomerStatus Status)
    {
        var customer = await FindCustomer(Email);

        if (customer is null)
            return null;

        customer.Status = Status;
        customer = _context.Customers.Update(customer).Entity;
        await _context.SaveChangesAsync();

        return DtoAndReverseConverter.CustomerToCustomerDto(customer);
    }

    public async Task<object> DeleteCustomer(string Email)
    {
        var customer = await FindCustomer(Email);

        if(customer is null)
            return null;

        int validInvoices = _context.Invoices.Count(i => i.CustomerId == customer.Id);

        if (validInvoices > 0)
        {
            Log.Information($"Customer have {validInvoices} invoices, therefore, the customer with this email -> {customer.Email} cannot be deleted.");
            return null;
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        _memoryCache.Remove(customer.Email);
        return true;
    }

    public async Task<CustomerDTO> EditCustomer(
        string Email, string? Name,
        string? Address, string? PhoneNumber, string Password)
    {
        var customer = await FindCustomer(Email);

        if (customer is null)
            return null;

        var isValidPassword = BCrypt.Net.BCrypt.Verify(Password, customer.Password);
        if (isValidPassword)
            return null;

        customer.Name = Name is not null ? Name : customer.Name;
        customer.Address = Address is not null ? Address : customer.Address;
        customer.PhoneNumber = PhoneNumber is not null ? PhoneNumber : customer.PhoneNumber;
        customer.UpdatedAt = DateTimeOffset.UtcNow;

        customer = _context.Customers.Update(customer).Entity;
        await _context.SaveChangesAsync();
        Log.Information($"Customer's account with this email -> {customer.Email} is updated.");

        return DtoAndReverseConverter.CustomerToCustomerDto(customer);
    }

    public async Task<CustomerDTO> GetCustomer(string Email)
    {
        var customer = await FindCustomer(Email);

        if (customer is null)
            return null;

        return DtoAndReverseConverter.CustomerToCustomerDto(customer);
    }

    public async Task SetUserInfo(UserInfo userInfo)
    {
        _userInfo = userInfo;
    }

    public async Task<PaginationDTO<CustomerDTO>> GetCustomers(int page, int pageSize, string? search, OrderBy? orderBy)
    {
        var customersOfUser = _context.UserCustomerRelation
            .Where(u => u.UserId == _userInfo.UserId)
            .Select(u => u.CustomerId);

        IQueryable<Customer> query = _context.Customers.Where(c => customersOfUser.Contains(c.Id));

        // Search
        if(!string.IsNullOrEmpty(search))
            query = query.Where(c => c.Name.Contains(search));

        // Sorting
        if(OrderBy.Ascending == orderBy)
            query = query.OrderBy(c => _context.Invoices.Count(i => i.CustomerId == c.Id));

        else if (OrderBy.Descending == orderBy)
            query = query.OrderByDescending(c => _context.Invoices.Count(i => i.CustomerId == c.Id));

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

    private async Task<Customer> FindCustomer(string Email)
    {
        if(_memoryCache.TryGetValue(Email, out Customer customer))
            return customer;

        customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == Email);

        if (customer is null)
        {
            Log.Information($"Customer didn't find with this email -> {customer.Email}.");
            return null;
        }

        var isThisCustomerOfUser = _context.UserCustomerRelation
            .Any(u => u.UserId == _userInfo.UserId && u.CustomerId == customer.Id);

        if (!isThisCustomerOfUser)
        {
            Log.Information($"Customer with this email -> {customer.Email} is not belong to this user.");
            return null;
        }

        _memoryCache.Set(customer.Email, customer, TimeSpan.FromMinutes(10));
        return customer;
    }
}