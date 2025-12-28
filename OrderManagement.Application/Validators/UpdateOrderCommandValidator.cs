using FluentValidation;
using OrderManagement.Application.UseCases.Orders.Commands;

namespace OrderManagement.Application.Validators;

public class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
{
    public UpdateOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Order ID is required");

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.CustomerName) ||
                       !string.IsNullOrWhiteSpace(x.CustomerEmail) ||
                       x.Status.HasValue)
            .WithMessage("At least one field must be provided for update");

        When(x => !string.IsNullOrWhiteSpace(x.CustomerName), () =>
        {
            RuleFor(x => x.CustomerName)
                .MaximumLength(200).WithMessage("Customer name must not exceed 200 characters")
                .Must(ValidationHelpers.IsSanitized).WithMessage(ValidationHelpers.GetDangerousCharactersMessage());
        });

        When(x => !string.IsNullOrWhiteSpace(x.CustomerEmail), () =>
        {
            RuleFor(x => x.CustomerEmail)
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(256).WithMessage("Email must not exceed 256 characters");
        });
    }
}
