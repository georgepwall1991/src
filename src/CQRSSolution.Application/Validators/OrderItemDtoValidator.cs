using CQRSSolution.Application.DTOs;
using FluentValidation;

namespace CQRSSolution.Application.Validators;

/// <summary>
///     Validator for <see cref="OrderItemDto" />.
/// </summary>
public class OrderItemDtoValidator : AbstractValidator<OrderItemDto>
{
    public OrderItemDtoValidator()
    {
        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative.");
    }
}