// Required for OrderItemDto

using CQRSSolution.Application.Commands.CreateOrder;
using FluentValidation;

namespace CQRSSolution.Application.Validators;

/// <summary>
///     Validator for <see cref="CreateOrderCommand" />.
/// </summary>
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(200).WithMessage("Customer name cannot exceed 200 characters.");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item.")
            .Must(items => items != null && items.Count > 0)
            .WithMessage("Order must contain at least one item."); // Redundant with NotEmpty, but good for clarity

        // Validate each item in the Items list
        RuleForEach(x => x.Items).SetValidator(new OrderItemDtoValidator());
    }
}