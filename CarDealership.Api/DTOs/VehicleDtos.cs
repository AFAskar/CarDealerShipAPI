using System.ComponentModel.DataAnnotations;

namespace CarDealership.Api.DTOs;

public record CreateVehicleRequest(
    [Required] string Make,
    [Required] string Model,
    [Range(1900, 2100)] int Year,
    [Range(0, double.MaxValue)] decimal Price
);

public record UpdateVehicleRequest(
    [Required] string Make,
    [Required] string Model,
    [Range(1900, 2100)] int Year,
    [Range(0, double.MaxValue)] decimal Price,
    bool IsAvailable
);

public record VehicleDto(
    Guid Id,
    string Make,
    string Model,
    int Year,
    decimal Price,
    bool IsAvailable,
    DateTime CreatedAt
);
