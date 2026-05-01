using System.Text.Json.Serialization;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QueueSystem.Api.Auth;
using QueueSystem.Api.Data;
using QueueSystem.Api.Models;

namespace QueueSystem.Api.Controllers;

public record LoginRequest(
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("password")] string Password);

public record LoginResponse(
    [property: JsonPropertyName("accessToken")] string AccessToken,
    [property: JsonPropertyName("expiresAtUtc")] DateTime ExpiresAtUtc,
    [property: JsonPropertyName("staffId")] int StaffId,
    [property: JsonPropertyName("staffName")] string StaffName,
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("branchId")] int BranchId,
    [property: JsonPropertyName("branchName")] string BranchName);

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtTokenService _tokens;

    public AuthController(AppDbContext db, JwtTokenService tokens)
    {
        _db = db;
        _tokens = tokens;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest body, CancellationToken ct)
    {
        var email = body.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(body.Password))
            return BadRequest("Email and password are required.");

        var staff = await _db.StaffMembers
            .Include(s => s.Branch)
            .FirstOrDefaultAsync(s => s.LoginEmail != null && s.LoginEmail.ToLower() == email, ct);
        if (staff?.PasswordHash is null || staff.Branch is null)
            return Unauthorized("Invalid email or password.");

        if (!BCrypt.Net.BCrypt.Verify(body.Password, staff.PasswordHash))
            return Unauthorized("Invalid email or password.");

        var role = staff.Role == StaffRole.Manager ? AuthRoles.Manager : AuthRoles.Staff;
        var (token, exp) = _tokens.CreateToken(staff, staff.Branch);
        return Ok(new LoginResponse(token, exp, staff.Id, staff.Name, role, staff.Branch.Id, staff.Branch.Name));
    }
}
