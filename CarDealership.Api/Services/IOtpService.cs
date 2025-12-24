using CarDealership.Api.Models;

namespace CarDealership.Api.Services;

public interface IOtpService
{
    Task<string> GenerateOtpAsync(User user);
    Task<bool> ValidateOtpAsync(User user, string code);
}
