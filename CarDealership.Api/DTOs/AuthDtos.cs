using System.ComponentModel.DataAnnotations;

namespace CarDealership.Api.DTOs;

public record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    string Role = "Customer" // Default to Customer, can be "Admin"
);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password
);

public record OtpVerificationRequest(
    [Required, EmailAddress] string Email,
    [Required] string Code,
    [Required] string Action // e.g., "Login", "Register", "Purchase", "UpdateVehicle"
);

public record AuthResponse(
    string Token,
    string Message,
    bool RequiresOtp = false
);

public record UserDto(
    Guid Id,
    string Email,
    string Role,
    DateTime CreatedAt
);
