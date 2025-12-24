using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CarDealership.Api.DTOs;
using CarDealership.Api.Models;
using CarDealership.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CarDealership.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly IOtpService _otpService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(UserManager<User> userManager, IOtpService otpService, IConfiguration configuration, ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _otpService = otpService;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var userExists = await _userManager.FindByEmailAsync(request.Email);
        if (userExists != null)
            return BadRequest("User already exists");

        var user = new User
        {
            Email = request.Email,
            UserName = request.Email,
            Role = request.Role,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        // Generate OTP for registration verification
        var code = await _otpService.GenerateOtpAsync(user);

        // Simulate sending OTP
        _logger.LogInformation("[OTP] Registration OTP for {Email}: {Code}", user.Email, code);

        return Ok(new AuthResponse(null!, "User created. Please verify OTP to complete registration.", true));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, [FromHeader(Name = "X-OTP")] string? otpCode)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized("Invalid credentials");

        if (!string.IsNullOrEmpty(otpCode))
        {
            var isValid = await _otpService.ValidateOtpAsync(user, otpCode);
            if (!isValid) return BadRequest("Invalid or expired OTP");

            var token = GenerateJwtToken(user);
            return Ok(new AuthResponse(token, "Login successful", false));
        }

        // Generate OTP for login
        var code = await _otpService.GenerateOtpAsync(user);

        // Simulate sending OTP
        _logger.LogInformation("[OTP] Login OTP for {Email}: {Code}", user.Email, code);

        return Ok(new AuthResponse(null!, "OTP sent. Please verify to login.", true));
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] OtpVerificationRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return NotFound("User not found");

        var isValid = await _otpService.ValidateOtpAsync(user, request.Code);
        if (!isValid)
            return BadRequest("Invalid or expired OTP");

        // If action is Login or Register, we issue a token
        if (request.Action.Equals("Login", StringComparison.OrdinalIgnoreCase) ||
            request.Action.Equals("Register", StringComparison.OrdinalIgnoreCase))
        {
            var token = GenerateJwtToken(user);
            return Ok(new AuthResponse(token, "Authentication successful", false));
        }

        // For other actions, we might just return success, 
        // and the client will use the fact that they passed this check 
        // (though for stateless APIs, usually the action itself should be performed here or the OTP passed to the action endpoint).
        // For this challenge, we'll assume this endpoint is mainly for Auth. 
        // Protected actions like "Purchase" will likely need to handle OTP verification internally or accept the OTP in the request.

        return Ok(new { Message = "OTP Verified successfully" });
    }

    private string GenerateJwtToken(User user)
    {
        var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName!),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            expires: DateTime.Now.AddHours(3),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
