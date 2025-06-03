using System.ComponentModel.DataAnnotations;

namespace CQRSSolution.Application.DTOs;

/// <summary>
/// Data Transfer Object for creating an order item.
/// </summary>
public class CreateOrderItemDto
{
    /// <summary>
    /// Gets or sets the name of the product.
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quantity of the product.
    /// Must be greater than 0.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the price per unit of the product.
    /// Must be greater than or equal to 0.
    /// </summary>
    [Range(0, (double)decimal.MaxValue, ErrorMessage = "Unit price must be greater than or equal to 0.")]
    public decimal UnitPrice { get; set; }
} 