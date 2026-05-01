using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using QueueSystem.Api.Models;

namespace QueueSystem.Api.Auth;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Key { get; set; } = "";
    public string Issuer { get; set; } = "QueueSystem.Api";
    public string Audience { get; set; } = "QueueSystem.Web";
    public int ExpireMinutes { get; set; } = 720;
}

public class JwtTokenService(IOptions<JwtOptions> options)
{
    private readonly JwtOptions _opt = options.Value;

    public (string Token, DateTime ExpiresUtc) CreateToken(Staff staff, Branch branch)
    {
        var role = staff.Role == StaffRole.Manager ? AuthRoles.Manager : AuthRoles.Staff;
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, staff.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(AuthRoles.BranchIdClaim, branch.Id.ToString()),
            new Claim(AuthRoles.StaffNameClaim, staff.Name),
            new Claim(ClaimTypes.Role, role),
            new Claim("branch_name", branch.Name)
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddMinutes(Math.Clamp(_opt.ExpireMinutes, 30, 60 * 24 * 7));
        var jwt = new JwtSecurityToken(
            issuer: _opt.Issuer,
            audience: _opt.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);
        var token = new JwtSecurityTokenHandler().WriteToken(jwt);
        return (token, jwt.ValidTo);
    }
}
