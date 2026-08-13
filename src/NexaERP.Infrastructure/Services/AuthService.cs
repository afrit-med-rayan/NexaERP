using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NexaERP.Application.DTOs.Auth;
using NexaERP.Application.Interfaces;
using NexaERP.Domain.Entities;
using NexaERP.Domain.Exceptions;
using NexaERP.Infrastructure.Data;

namespace NexaERP.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email.ToLower()))
            throw new BusinessException($"Email '{request.Email}' is already registered.");

        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == request.Role)
            ?? throw new NotFoundException($"Role '{request.Role}' not found.");

        var user = new User
        {
            FullName = request.FullName,
            Email    = request.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        _db.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id });
        await _db.SaveChangesAsync();

        return GenerateToken(user, new[] { role.Name });
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLower())
            ?? throw new NotFoundException("Invalid email or password.");

        if (!user.IsActive)
            throw new BusinessException("Account is disabled.");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new NotFoundException("Invalid email or password.");

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToArray();
        return GenerateToken(user, roles);
    }

    public async Task<UserProfileResponse> GetProfileAsync(Guid userId)
    {
        var user = await _db.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new NotFoundException("User not found.");

        return new UserProfileResponse(
            user.Id,
            user.FullName,
            user.Email,
            user.UserRoles.Select(ur => ur.Role.Name),
            user.CreatedAt);
    }

    // ── Private ────────────────────────────────────────────────────────────────

    private AuthResponse GenerateToken(User user, IEnumerable<string> roles)
    {
        var jwtSection  = _config.GetSection("Jwt");
        var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
        var creds       = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiryMins  = int.Parse(jwtSection["ExpiryMinutes"] ?? "60");
        var expiresAt   = DateTime.UtcNow.AddMinutes(expiryMins);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Name,  user.FullName),
            new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var token = new JwtSecurityToken(
            issuer:   jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims:   claims,
            expires:  expiresAt,
            signingCredentials: creds);

        return new AuthResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            user.FullName,
            user.Email,
            roles,
            expiresAt);
    }
}
