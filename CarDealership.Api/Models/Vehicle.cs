using System.ComponentModel.DataAnnotations;

public class Vehicle
{
    public Guid Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Make { get; set; } = default!;

    [Required]
    [MaxLength(100)]
    public string Model { get; set; } = default!;

    [Range(1900, 2100)]
    public int Year { get; set; }

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    public bool IsAvailable { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
