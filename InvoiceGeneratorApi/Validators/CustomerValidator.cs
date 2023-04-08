using FluentValidation;
using InvoiceGeneratorApi.DTO;

namespace InvoiceGeneratorApi.Validators;


public class CustomerValidator : AbstractValidator<CustomerDTO>
{
    public CustomerValidator()
    {
        //RuleFor(customer => customer.Id)
        //    .GreaterThan(0).WithMessage("Id must be greater than zero.");

        RuleFor(customer => customer.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(customer => customer.Address)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(200).WithMessage("Address must not exceed 200 characters.");

        RuleFor(customer => customer.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(customer => customer.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .MaximumLength(16).WithMessage("Your password length must not exceed 16.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^\+994\d{10}$").WithMessage("Phone number must be in the format of +994XXXXXXXXXX");

        RuleFor(customer => customer.Status)
            .IsInEnum().WithMessage("Invalid customer status.");
    }
}