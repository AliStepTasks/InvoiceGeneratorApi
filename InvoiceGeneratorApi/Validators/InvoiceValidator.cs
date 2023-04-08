using FluentValidation;
using InvoiceGeneratorApi.DTO;

namespace InvoiceGeneratorApi.Validators
{
    public class InvoiceValidator : AbstractValidator<InvoiceDTO>
    {
        public InvoiceValidator()
        {
            //RuleFor(invoice => invoice.Id)
            //    .GreaterThan(0).WithMessage("Id must be greater than zero.");

            RuleFor(invoice => invoice.CustomerId)
                .GreaterThan(0).WithMessage("CustomerId must be greater than zero.");

            RuleFor(invoice => invoice.StartDate)
                .LessThanOrEqualTo(invoice => invoice.EndDate).WithMessage("StartDate must not be later than EndDate.");

            RuleFor(invoice => invoice.EndDate)
                .GreaterThanOrEqualTo(invoice => invoice.StartDate).WithMessage("EndDate must not be earlier than StartDate.");

            RuleFor(invoice => invoice.Rows)
                .NotNull().WithMessage("Rows is required.")
                .Must(rows => rows.Length > 0).WithMessage("At least one row is required.");

            RuleForEach(invoice => invoice.Rows)
                .SetValidator(new InvoiceRowDTOValidator());

            RuleFor(invoice => invoice.TotalSum)
                .GreaterThanOrEqualTo(0).WithMessage("TotalSum must not be negative.");

            RuleFor(invoice => invoice.Comment)
                .MaximumLength(500).WithMessage("Comment must not exceed 500 characters.");

            RuleFor(invoice => invoice.Status)
                .IsInEnum().WithMessage("Invalid invoice status.");

            RuleFor(invoice => invoice.CreatedAt)
                .LessThanOrEqualTo(DateTimeOffset.UtcNow).WithMessage("CreatedAt must not be in the future.");
        }
    }
}