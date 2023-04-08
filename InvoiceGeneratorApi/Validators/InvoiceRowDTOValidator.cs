using FluentValidation;
using InvoiceGeneratorApi.DTO;

namespace InvoiceGeneratorApi.Validators;

public class InvoiceRowDTOValidator : AbstractValidator<InvoiceRowDTO>
{
    public InvoiceRowDTOValidator()
    {
        RuleFor(x => x.Service)
            .NotEmpty().WithMessage("Service name is required.")
            .MaximumLength(50).WithMessage("Service name cannot exceed 50 characters.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");

        RuleFor(x => x.Sum)
            .GreaterThan(0).WithMessage("Sum must be greater than zero.")
            .Equal(x => x.Quantity * x.Amount).WithMessage("Sum should be equal to Quantity * Amount.");
    }
}