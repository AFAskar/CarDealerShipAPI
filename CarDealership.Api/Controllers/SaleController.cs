using CarDealership.Api.Data;
using CarDealership.Api.DTOs;
using CarDealership.Api.Models;
using CarDealership.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarDealership.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SaleController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IOtpService _otpService;

    public SaleController(AppDbContext context, UserManager<User> userManager, IOtpService otpService)
    {
        _context = context;
        _userManager = userManager;
        _otpService = otpService;
    }

    [HttpPost("request")]
    [Authorize(Roles = "Customer")]
    public async Task<IActionResult> RequestPurchase([FromBody] PurchaseRequest request, [FromHeader(Name = "X-OTP")] string? otpCode)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        // 1. Check if vehicle exists and is available
        var vehicle = await _context.Vehicles.FindAsync(request.VehicleId);
        if (vehicle == null) return NotFound("Vehicle not found");
        if (!vehicle.IsAvailable) return BadRequest("Vehicle is not available");

        // 2. OTP Check
        if (string.IsNullOrEmpty(otpCode))
        {
            var code = await _otpService.GenerateOtpAsync(user);
            Console.WriteLine($"[OTP] Purchase Request OTP for {user.Email}: {code}");
            return StatusCode(428, new { Message = "OTP required. Code sent to console.", RequiresOtp = true });
        }

        var isValid = await _otpService.ValidateOtpAsync(user, otpCode);
        if (!isValid) return BadRequest("Invalid or expired OTP");

        // 3. Create Sale Request
        var sale = new Sale
        {
            UserId = user.Id,
            VehicleId = vehicle.Id,
            PriceAtPurchase = vehicle.Price,
            PurchasedAt = DateTime.UtcNow,
            Status = SaleStatus.Pending
        };

        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Purchase request submitted successfully", SaleId = sale.Id });
    }

    [HttpPost("process")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ProcessSale([FromBody] ProcessSaleRequest request)
    {
        var sale = await _context.Sales
            .Include(s => s.Vehicle)
            .FirstOrDefaultAsync(s => s.Id == request.SaleId);

        if (sale == null) return NotFound("Sale request not found");

        if (sale.Status != SaleStatus.Pending)
            return BadRequest($"Sale is already {sale.Status}");

        if (request.Approve)
        {
            // Check if vehicle is still available
            if (!sale.Vehicle.IsAvailable)
            {
                sale.Status = SaleStatus.Rejected;
                await _context.SaveChangesAsync();
                return BadRequest("Vehicle is no longer available");
            }

            sale.Status = SaleStatus.Completed;
            sale.Vehicle.IsAvailable = false; // Mark vehicle as sold
        }
        else
        {
            sale.Status = SaleStatus.Rejected;
        }

        await _context.SaveChangesAsync();

        return Ok(new { Message = $"Sale request {sale.Status}", SaleId = sale.Id });
    }

    [HttpGet("history")]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<IEnumerable<SaleDto>>> GetPurchaseHistory()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var sales = await _context.Sales
            .Include(s => s.Vehicle)
            .Where(s => s.UserId == user.Id)
            .OrderByDescending(s => s.PurchasedAt)
            .Select(s => new SaleDto(
                s.Id,
                s.VehicleId,
                $"{s.Vehicle.Year} {s.Vehicle.Make} {s.Vehicle.Model}",
                s.PriceAtPurchase,
                s.PurchasedAt
            )) // Note: You might want to add Status to SaleDto if needed
            .ToListAsync();

        return Ok(sales);
    }
}
