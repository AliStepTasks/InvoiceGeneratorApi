using InvoiceGeneratorApi.DTO;
using InvoiceGeneratorApi.Models;

namespace InvoiceGeneratorApi.Services;

public static class DtoAndReverseConverter
{

    /// <summary>
    /// Converts CustomerDTO to Customer
    /// </summary>
    /// <param name="customerDto"></param>
    /// <returns></returns>
    public static Customer CustomerDtoToCustomer(CustomerDTO customerDto)
    {
        var customer = new Customer
        {
            Id = customerDto.Id,
            Name = customerDto.Name,
            Address = customerDto.Address,
            Email = customerDto.Email,
            Password = customerDto.Password,
            PhoneNumber = customerDto.PhoneNumber,
            Status = customerDto.Status,
        };
        return customer;
    }

    /// <summary>
    /// Converts Customer to CustomerDTO
    /// </summary>
    /// <param name="customer"></param>
    /// <returns></returns>
    public static CustomerDTO CustomerToCustomerDto(Customer customer)
    {
        var customerDto = new CustomerDTO
        {
            Id = customer.Id,
            Name = customer.Name,
            Address = customer.Address,
            Email = customer.Email,
            Password = customer.Password,
            PhoneNumber = customer.PhoneNumber,
            Status = customer.Status
        };
        return customerDto;
    }

    /// <summary>
    /// Converts User to UserDTO
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static UserDTO UserToUserDto(User user)
    {
        var userDTO = new UserDTO
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Address = user.Address,
            Password = user.Password,
            PhoneNumber = user.PhoneNumber
        };

        return userDTO;
    }

    /// <summary>
    /// Converts UserDTO to User
    /// </summary>
    /// <param name="userDTO"></param>
    /// <returns></returns>
    public static User UserDtoToUser(UserDTO userDTO)
    {
        var user = new User
        {
            Id = userDTO.Id,
            Name = userDTO.Name,
            Email = userDTO.Email,
            Address = userDTO.Address,
            Password = userDTO.Password,
            PhoneNumber = userDTO.PhoneNumber
        };

        return user;
    }
    /// <summary>
    /// Converts Invoice to InvoiceDTO
    /// </summary>
    /// <param name="invoice"></param>
    /// <returns></returns>
    public static InvoiceDTO InvoiceToInvoiceDto(Invoice invoice)
    {
        var invoiceDTO = new InvoiceDTO
        {
            Id = invoice.Id,
            CustomerId = invoice.CustomerId,
            StartDate = invoice.StartDate,
            EndDate = invoice.EndDate,
            Rows = invoice.Rows,
            TotalSum = invoice.TotalSum,
            Comment = invoice.Comment,
            Status = invoice.Status,
            CreatedAt = invoice.CreatedAt
        };

        return invoiceDTO;
    }

    /// <summary>
    /// Converts InvoiceDTO to Invoice
    /// </summary>
    /// <param name="invoiceDTO"></param>
    /// <returns></returns>
    public static Invoice InvoiceDtoToInvoice(InvoiceDTO invoiceDTO)
    {
        var invoice = new Invoice
        {
            Id = invoiceDTO.Id,
            CustomerId = invoiceDTO.CustomerId,
            StartDate = invoiceDTO.StartDate,
            EndDate = invoiceDTO.EndDate,
            Rows = invoiceDTO.Rows,
            TotalSum = invoiceDTO.TotalSum,
            Comment = invoiceDTO.Comment,
            Status = invoiceDTO.Status,
            CreatedAt = invoiceDTO.CreatedAt
        };

        return invoice;
    }

    /// <summary>
    /// Converts InvoiceRow to InvoiceRowDTO
    /// </summary>
    /// <param name="invoice"></param>
    /// <returns></returns>
    public static InvoiceRowDTO InvoiceRowToInvoiceRowDto(InvoiceRow invoiceRow)
    {
        var invoiceRowDTO = new InvoiceRowDTO
        {
            Id = invoiceRow.Id,
            Service = invoiceRow.Service,
            Quantity = invoiceRow.Quantity,
            Amount = invoiceRow.Amount,
            Sum = invoiceRow.Sum
        };

        return invoiceRowDTO;
    }

    /// <summary>
    /// Converts InvoiceRowDTO to InvoiceRow
    /// </summary>
    /// <param name="invoiceDTO"></param>
    /// <returns></returns>
    public static InvoiceRow InvoiceRowDtoToInvoiceRow(InvoiceRowDTO invoiceRowDTO)
    {
        var invoiceRow = new InvoiceRow
        {
            Id = invoiceRowDTO.Id,
            Service = invoiceRowDTO.Service,
            Quantity = invoiceRowDTO.Quantity,
            Amount = invoiceRowDTO.Amount,
            Sum = invoiceRowDTO.Sum
        };

        return invoiceRow;
    }
}