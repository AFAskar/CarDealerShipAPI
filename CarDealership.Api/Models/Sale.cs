using System.ComponentModel.DataAnnotations;

namespace CarDealership.Api.Models;

public class Sale
{
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public Guid VehicleId { get; set; }

    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;

    public decimal PriceAtPurchase { get; set; }

    // Navigation
    public User User { get; set; } = default!;
    public Vehicle Vehicle { get; set; } = default!;
}
