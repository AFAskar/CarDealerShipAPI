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
public class VehicleController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IOtpService _otpService;
    private readonly ILogger<VehicleController> _logger;

    public VehicleController(AppDbContext context, UserManager<User> userManager, IOtpService otpService, ILogger<VehicleController> logger)
    {
        _context = context;
        _userManager = userManager;
        _otpService = otpService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<VehicleDto>>> GetVehicles(
        [FromQuery] string? make,
        [FromQuery] string? model,
        [FromQuery] int? year,
        [FromQuery] decimal? maxPrice,
        [FromQuery] bool onlyAvailable = true)
    {
        var query = _context.Vehicles.AsQueryable();

        if (onlyAvailable)
            query = query.Where(v => v.IsAvailable);

        if (!string.IsNullOrEmpty(make))
            query = query.Where(v => v.Make.Contains(make));

        if (!string.IsNullOrEmpty(model))
            query = query.Where(v => v.Model.Contains(model));

        if (year.HasValue)
            query = query.Where(v => v.Year == year);

        if (maxPrice.HasValue)
            query = query.Where(v => v.Price <= maxPrice);

        var vehicles = await query
            .Select(v => new VehicleDto(v.Id, v.Make, v.Model, v.Year, v.Price, v.IsAvailable, v.CreatedAt))
            .ToListAsync();

        return Ok(vehicles);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VehicleDto>> GetVehicle(Guid id)
    {
        var v = await _context.Vehicles.FindAsync(id);

        if (v == null)
        {
            return NotFound();
        }

        return new VehicleDto(v.Id, v.Make, v.Model, v.Year, v.Price, v.IsAvailable, v.CreatedAt);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<VehicleDto>> CreateVehicle(CreateVehicleRequest request)
    {
        var vehicle = new Vehicle
        {
            Make = request.Make,
            Model = request.Model,
            Year = request.Year,
            Price = request.Price,
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetVehicle), new { id = vehicle.Id },
            new VehicleDto(vehicle.Id, vehicle.Make, vehicle.Model, vehicle.Year, vehicle.Price, vehicle.IsAvailable, vehicle.CreatedAt));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateVehicle(Guid id, UpdateVehicleRequest request, [FromHeader(Name = "X-OTP")] string? otpCode)
    {
        var vehicle = await _context.Vehicles.FindAsync(id);
        if (vehicle == null)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        // OTP Check
        if (string.IsNullOrEmpty(otpCode))
        {
            // Generate and send OTP
            var code = await _otpService.GenerateOtpAsync(user);
            _logger.LogInformation("[OTP] Update Vehicle OTP for {Email}: {Code}", user.Email, code);

            return StatusCode(428, new { Message = "OTP required. Code sent to console.", RequiresOtp = true });
        }

        // Validate OTP
        var isValid = await _otpService.ValidateOtpAsync(user, otpCode);
        if (!isValid)
        {
            return BadRequest("Invalid or expired OTP");
        }

        // Perform Update
        vehicle.Make = request.Make;
        vehicle.Model = request.Model;
        vehicle.Year = request.Year;
        vehicle.Price = request.Price;
        vehicle.IsAvailable = request.IsAvailable;

        await _context.SaveChangesAsync();

        return NoContent();
    }
}
