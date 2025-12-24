using System.Security.Cryptography;
using System.Text;
using CarDealership.Api.Data;
using CarDealership.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace CarDealership.Api.Services;

public class OtpService : IOtpService
{
    private readonly AppDbContext _context;

    public OtpService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateOtpAsync(User user)
    {
        // 1. Generate a 6-digit code
        var code = Random.Shared.Next(100000, 999999).ToString();

        // 2. Hash the code
        var codeHash = HashCode(code);

        // 3. Create OtpCode entity
        var otp = new OtpCode
        {
            UserId = user.Id,
            CodeHash = codeHash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5), // 5 minutes expiration
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        // 4. Save to DB
        _context.OtpCodes.Add(otp);
        await _context.SaveChangesAsync();

        // 5. Return the plain code (to be sent via SMS/Email)
        return code;
    }

    public async Task<bool> ValidateOtpAsync(User user, string code)
    {
        var codeHash = HashCode(code);

        // Find the most recent valid OTP for this user
        var otp = await _context.OtpCodes
            .Where(o => o.UserId == user.Id && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (otp == null)
        {
            return false;
        }

        // Verify hash
        if (otp.CodeHash != codeHash)
        {
            return false;
        }

        // Mark as used
        otp.IsUsed = true;
        await _context.SaveChangesAsync();

        return true;
    }

    private static string HashCode(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}
