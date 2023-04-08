using InvoiceGeneratorApi.Models;
using Bogus;
using InvoiceGeneratorApi.DTO;
using InvoiceGeneratorApi.Data;
using InvoiceGeneratorApi.Enums;

namespace InvoiceGeneratorApi.Services;

public static class SeedDb
{
    /// <summary>
    /// Seeding database with customers
    /// </summary>
    /// <param name="numberOfCustomers"></param>
    /// <returns></returns>
    public static IEnumerable<Customer> CustomerSeed(int numberOfCustomers)
    {
        var customerList = new List<Customer>();
        for (int i = 0; i < numberOfCustomers; i++)
        {
            var faker = new Faker();
            var customer = new Customer
            {
                Name = faker.Name.FirstName(),
                Address = faker.Address.StreetAddress(),
                Email = faker.Internet.Email(),
                Password = BCrypt.Net.BCrypt.HashPassword(faker.Internet.Password()),
                PhoneNumber = faker.Phone.PhoneNumber(),
                Status = faker.PickRandom<CustomerStatus>(),
                CreatedAt = faker.Date.PastOffset(),
                UpdatedAt = faker.Date.PastOffset(),
                DeletedAt = faker.Date.PastOffset()
            };
            customerList.Add(customer);
        }
        return customerList;
    }

    /// <summary>
    /// Seeding database with invoices
    /// </summary>
    /// <param name="numberOfInvoices"></param>
    /// <param name="numberOfRows"></param>
    /// <returns></returns>
    public static IEnumerable<Invoice> InvoiceSeed(int numberOfInvoices, int numberOfRows, int[] customersId)
    {
        var invoiceList = new List<Invoice>();
        for (int i = 0; i < numberOfInvoices; i++)
        {
            var faker = new Faker();
            var rows = new List<InvoiceRowDTO>();
            var invoice = new Invoice
            {
                CustomerId = faker.PickRandom(customersId),
                StartDate = faker.Date.RecentOffset(),
                EndDate = faker.Date.FutureOffset(),
                TotalSum = 0,
                Comment = faker.Commerce.ProductDescription(),
                Status = faker.PickRandom<InvoiceStatus>(),
                CreatedAt = faker.Date.PastOffset(),
                UpdatedAt = faker.Date.RecentOffset(),
                DeletedAt = faker.Date.FutureOffset()
            };
            var rowCount = faker.Random.Number(numberOfRows);

            for (int j = 0; j < rowCount; j++)
            {
                var row = new InvoiceRowDTO
                {
                    Service = faker.Commerce.ProductName(),
                    Quantity = faker.Random.Int(1, 15),
                    Amount = Decimal.Parse(faker.Commerce.Price())
                };
                row.Sum = row.Quantity * row.Amount;
                invoice.TotalSum += row.Sum;
                rows.Add(row);
            }
            invoice.Rows = rows.ToArray();
            invoiceList.Add(invoice);
        }
        return invoiceList;
    }

    /// <summary>
    /// Seeding database with users
    /// </summary>
    /// <param name="numberOfUser"></param>
    /// <returns></returns>
    public static IEnumerable<User> UserSeed(int numberOfUsers)
    {
        var userList = new List<User>();
        for (int i = 0; i < numberOfUsers; i++)
        {
            var faker = new Faker();
            var user = new User
            {
                Name = faker.Name.FirstName(),
                Address = faker.Address.StreetAddress(),
                Email = faker.Internet.Email(),
                Password = BCrypt.Net.BCrypt.HashPassword(faker.Internet.Password()),
                PhoneNumber = faker.Phone.PhoneNumber(),
                CreatedAt = faker.Date.PastOffset(),
                UpdatedAt = faker.Date.PastOffset()
            };
            userList.Add(user);
        }
        return userList;
    }
}