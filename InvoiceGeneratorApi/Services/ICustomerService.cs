using InvoiceGeneratorApi.DTO;
using InvoiceGeneratorApi.DTO.Pagination;
using InvoiceGeneratorApi.Enums;
using InvoiceGeneratorApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceGeneratorApi.Services;

public interface ICustomerService
{
    Task<CustomerDTO> AddCustomer(CustomerDTO customerDTO);
    Task<CustomerDTO> EditCustomer(
        string Email, string? Name,
        string? Address, string? PhoneNumber, string Password);
    Task<CustomerDTO> ChangeCustomerStatus(string Email, CustomerStatus Status);
    Task<CustomerDTO> GetCustomer(string Email);
    Task<PaginationDTO<CustomerDTO>> GetCustomers(int page, int pageSize, string? search, OrderBy? orderBy);
    Task<object> DeleteCustomer(string Email);
}