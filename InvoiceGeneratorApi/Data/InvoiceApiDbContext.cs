using InvoiceGeneratorApi.Models;
using Microsoft.EntityFrameworkCore;
using InvoiceGeneratorApi.DTO;
using InvoiceGeneratorApi.Services;

namespace InvoiceGeneratorApi.Data;

public class InvoiceApiDbContext : DbContext
{
	public InvoiceApiDbContext(DbContextOptions<InvoiceApiDbContext> options) : base(options)
	{
		Database.EnsureCreated();
	}

    public DbSet<User> Users { get; set; }
	public DbSet<Customer> Customers { get; set; }
	public DbSet<Invoice> Invoices { get; set; }
	public DbSet<InvoiceRow> InvoiceRows { get; set; }
}