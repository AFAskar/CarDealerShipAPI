using CarDealership.Api.DTOs;
using CarDealership.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarDealership.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class UserController : ControllerBase
{
    private readonly UserManager<User> _userManager;

    public UserController(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetCustomers()
    {
        var users = await _userManager.Users
            .Where(u => u.Role == "Customer")
            .Select(u => new UserDto(u.Id, u.Email!, u.Role, u.CreatedAt))
            .ToListAsync();

        return Ok(users);
    }
}
