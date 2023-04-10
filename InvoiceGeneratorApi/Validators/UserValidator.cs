using FluentValidation;
using InvoiceGeneratorApi.DTO;
using InvoiceGeneratorApi.DTO.Auth;

namespace InvoiceGeneratorApi.Validators;

public class UserValidator : AbstractValidator<UserRegisterRequest>
{
    public UserValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(50).WithMessage("Name cannot exceed 50 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters.")
            .EmailAddress().WithMessage("Email address is invalid.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(16).WithMessage("Your password length must not exceed 16.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required")
            .Matches(@"^\+994\d{10}$").WithMessage("Phone number must be in the format of +994XXXXXXXXXX");
    }
}