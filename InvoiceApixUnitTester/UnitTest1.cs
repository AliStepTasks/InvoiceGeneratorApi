using InvoiceGeneratorApi.Data;
using InvoiceGeneratorApi.DTO.Auth;
using InvoiceGeneratorApi.Models;
using InvoiceGeneratorApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace InvoiceApixUnitTester;

public class UnitTest1
{
    private InvoiceApiDbContext _context;
    private MemoryCache _cache;
    public UnitTest1()
    {
        _context = new InvoiceApiDbContext(new DbContextOptionsBuilder<InvoiceApiDbContext>()
            .UseInMemoryDatabase("Test").Options);
        _cache = new MemoryCache(new MemoryCacheOptions());
    }


    [Fact]
    public async Task Create_User_Via_SeedDb()
    {
        var service = new UserService(_context, _cache);

        var testUser = SeedDb.UserSeed(1).FirstOrDefault();
        var dtoUser = await service.RegisterUser(new UserRegisterRequest
        {
            Name = testUser.Name,
            Address = testUser.Address,
            Email = testUser.Email,
            PhoneNumber = testUser.PhoneNumber,
            Password = testUser.Password,
        });

        Assert.NotNull(dtoUser);
    }

    [Fact]
    public async Task Get_User_AccordingTo_Email_And_Password()
    {
        var service = new UserService(_context, _cache);
        var testUser = SeedDb.UserSeed(1).FirstOrDefault();
         _context.Users.Add(testUser);
        await _context.SaveChangesAsync();

        var dtoUser = await service.LogInUser(testUser.Email, testUser.Password);
        Assert.NotNull(dtoUser);
    }

    [Fact]
    public async Task Hard_Delete_User_AccordingTo_Email_And_Password()
    {
        var service = new UserService(_context, _cache);
        var testUser = SeedDb.UserSeed(1).FirstOrDefault();
        _context.Users.Add(testUser);
        await _context.SaveChangesAsync();

        var deletedUser = await service.DeleteUser(testUser.Email, testUser.Password);
        Assert.NotNull(deletedUser);
    }

    [Fact]
    public async Task Create_Customer_Via_SeedDb()
    {
        var service = new CustomerService(_context, _cache);

        var testCustomer = SeedDb.CustomerSeed(1).FirstOrDefault();
        var dtoCustomer = await service.AddCustomer(DtoAndReverseConverter.CustomerToCustomerDto(testCustomer));

        Assert.NotNull(dtoCustomer);
    }

    [Fact]
    public async Task Get_Customer_AccordingTo_Email()
    {
        var service = new CustomerService(_context, _cache);
        var testCustomer = SeedDb.CustomerSeed(1).FirstOrDefault();
        _context.Customers.Add(testCustomer);
        await _context.SaveChangesAsync();

        var dtoCustomer = await service.GetCustomer(testCustomer.Email);
        Assert.NotNull(dtoCustomer);
    }

    [Fact]
    public async Task Hard_Delete_Customer_AccordingTo_Email()
    {
        var service = new CustomerService(_context, _cache);
        var testCustomer = SeedDb.CustomerSeed(1).FirstOrDefault();
        _context.Customers.Add(testCustomer);
        await _context.SaveChangesAsync();

        var deletedCustomer = await service.DeleteCustomer(testCustomer.Email);
        Assert.NotNull(deletedCustomer);
    }

    [Fact]
    public async Task Create_Invoice_Via_SeedDb()
    {
        var service = new InvoiceService(_context);
        var testInvoice = SeedDb.InvoiceSeed(1, 1, new int[] { 0 }).FirstOrDefault();

        var dtoInvoice = await service.CreateInvoice(DtoAndReverseConverter.InvoiceToInvoiceDto(testInvoice));

        Assert.NotNull(dtoInvoice);
    }

    [Fact]
    public async Task Get_Invoice_And_Rows_AccordingTo_Invoice_Id()
    {
        var service = new InvoiceService(_context);
        var testInvoice = SeedDb.InvoiceSeed(1, 1, new int[] { 0 }).FirstOrDefault();
        var rows = testInvoice.Rows.Select(s => DtoAndReverseConverter.InvoiceRowDtoToInvoiceRow(s));
        foreach (var row in rows)
            _context.InvoiceRows.Add(row);
        await _context.SaveChangesAsync();

        _context.Invoices.Add(testInvoice);
        await _context.SaveChangesAsync();

        var dtoInvoice = await service.GetInvoice(testInvoice.Id);

        Assert.NotNull(dtoInvoice);
    }

    [Fact]
    public async Task Hard_Delete_Invoice_AccordingTo_Invoice_Id()
    {
        var service = new InvoiceService(_context);
        var testInvoice = SeedDb.InvoiceSeed(1, 1, new int[] { 0 }).FirstOrDefault();
        var rows = testInvoice.Rows.Select(s => DtoAndReverseConverter.InvoiceRowDtoToInvoiceRow(s));
        foreach (var row in rows)
            _context.InvoiceRows.Add(row);
        await _context.SaveChangesAsync();

        _context.Invoices.Add(testInvoice);
        await _context.SaveChangesAsync();

        var deletedInvoice = await service.DeleteInvoice(testInvoice.Id);

        Assert.NotNull(deletedInvoice);
    }
}