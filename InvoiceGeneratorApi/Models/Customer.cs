using System.ComponentModel.DataAnnotations;
using System;
using Newtonsoft.Json.Linq;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using InvoiceGeneratorApi.Enums;

namespace InvoiceGeneratorApi.Models;

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Address { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string? PhoneNumber { get; set; }

    //[EnumDataType(typeof(CustomerStatus))]
    public CustomerStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset DeletedAt { get; set; }
}