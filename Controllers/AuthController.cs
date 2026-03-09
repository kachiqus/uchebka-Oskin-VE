using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using oskin_site.Data;
using oskin_site.Models;
using oskin_site.Services;

namespace oskin_site.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly PasswordHasherService _hasher;
    private readonly JwtTokenService _jwt;

    public AuthController(ApplicationDbContext db, PasswordHasherService hasher, JwtTokenService jwt)
    {
        _db = db;
        _hasher = hasher;
        _jwt = jwt;
    }

    public sealed record RegisterRequest(string Username, string Password, string? Role);
    public sealed record LoginRequest(string Username, string Password);

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Username/password required.");

        var username = req.Username.Trim();
        
        var exists = await _db.Set<User>()
            .AnyAsync(x => x.Username == username);

        if (exists) return Conflict("User already exists.");

        var user = new User
        {
            Username = username,
            PasswordHash = _hasher.Hash(req.Password),
            Role = string.IsNullOrWhiteSpace(req.Role) ? "User" : req.Role!.Trim()
        };

        _db.Set<User>().Add(user);
        await _db.SaveChangesAsync();

        return Ok(new { id = user.UsersId, username = user.Username, role = user.Role });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Username/password required.");

        var username = req.Username.Trim();

        var user = await _db.Set<User>()
            .FirstOrDefaultAsync(x => x.Username == username);

        if (user is null) return Unauthorized();

        if (!_hasher.Verify(req.Password, user.PasswordHash))
            return Unauthorized();

        var token = _jwt.CreateToken(
            userId: user.UsersId,
            username: user.Username,
            role: user.Role);

        return Ok(new { token });
    }
}


