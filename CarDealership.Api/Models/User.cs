using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CarDealership.Api.Models;

public class User : IdentityUser<Guid>
{
    [Required]
    public string Role { get; set; } = "Customer"; // Admin | Customer

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
