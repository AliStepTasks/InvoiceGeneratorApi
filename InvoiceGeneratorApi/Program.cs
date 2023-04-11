using Microsoft.EntityFrameworkCore;
using InvoiceGeneratorApi.Data;
using System.Text.Json.Serialization;
using InvoiceGeneratorApi.Filters;
using InvoiceGeneratorApi.Services;
using FluentValidation.AspNetCore;
using InvoiceGeneratorApi.Interfaces;
using InvoiceGeneratorApi.Auth;
using InvoiceGeneratorApi.Validators;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);


//#pragma warning disable CS0618 // Type or member is obsolete
//builder.Services
//    .AddControllers()
//    .AddFluentValidation(x =>
//    {
//        x.ImplicitlyValidateChildProperties = true!;
//        x.RegisterValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
//    });
//#pragma warning restore CS0618 // Type or member is obsolete




// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(options =>
{
    //Enum serialization
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddFluentValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CustomerValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IServiceInvoice, InvoiceService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddTransient<InvoiceApiDbContext>();


builder.Services.AuthenticationAndAuthorization(builder.Configuration);
builder.Services.AddSwagger();

builder.Services.AddSwaggerGen(c =>
{
    //Filter for getting Enum values as string
    c.SchemaFilter<EnumSchemaFilter>();
});

builder.Services.AddDbContext<InvoiceApiDbContext>(
        options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("InvoiceApiConnection"))
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();