using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using EasyStore.Common.Requests.Auth;
using EasyStore.Common.Responses.Auth;
using EasyStore.Core.Exceptions;
using EasyStore.Core.StaticClasses;
using EasyStore.Data;
using EasyStore.Data.Entities;
using EasyStore.Data.Interfaces;
using EasyStore.Domain.Interfaces;

namespace EasyStore.Domain.Services;

public class AuthService(AppDbContext context, IUserRepository userRepository, IHttpContextAccessor httpContextAccessor) : IAuthService
{
    public async Task<RegisterUserResponse?> RegisterAsync(RegisterUserRequest request)
    {
        if (await userRepository.IsEmailAlreadyUsed(request.Email))
        {
            throw new AppException("Email is already in use.").SetStatusCode(409);
        }

        User user = new User()
        {
            Email = request.Email,
            Names = request.Names,
            Phone = request.Phone,
            Role = Roles.Admin
        };

        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, request.Password);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return new RegisterUserResponse { Id = user.Id };
    }

    public async Task<TokenResponse?> LoginAsync(LoginUserRequest request)
    {
        User? user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user is null)
            return null;

        PasswordVerificationResult result = new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return null;

        return await CreateTokenResponse(user);
    }

    public async Task<TokenResponse?> RefreshTokensAsync(RefreshTokenRequest request)
    {
        User? user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);
        if (user is null)
            return null;

        return await CreateTokenResponse(user);
    }

    public async Task<bool> LogoutAsync()
    {
        string? userIdStr = await GetCurrentUserId();
        if (string.IsNullOrEmpty(userIdStr))
            return false;

        Guid currentUserId = Guid.Parse(userIdStr);
        User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);

        if (user is null)
            throw new AppException("User not found.").SetStatusCode(404);

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await context.SaveChangesAsync();

        return true;
    }

    private async Task<User?> ValidateRefreshTokenAsync(Guid userId, string refreshToken)
    {
        User? user = await context.Users.FindAsync(userId);
        if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return null;
        }
        return user;
    }

    private async Task<TokenResponse> CreateTokenResponse(User user)
    {
        return new TokenResponse
        {
            AccessToken = CreateToken(user),
            RefreshToken = await GenerateAndSaveRefreshTokenAsync(user)
        };
    }

    private string GenerateRefreshToken()
    {
        byte[] randomNumber = new byte[32];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
    {
        string refreshToken = GenerateRefreshToken();
        user.RefreshToken = refreshToken;

        string? expiryDays = Environment.GetEnvironmentVariable("REFRESH_TOKEN_EXPIRY_DAYS");
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(int.Parse(expiryDays ?? "7"));

        await context.SaveChangesAsync();
        return refreshToken;
    }

    private string CreateToken(User user)
    {
        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role)
        };

        string? secret = Environment.GetEnvironmentVariable("JWT_TOKEN_SECRET");
        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!));
        SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        JwtSecurityToken tokenDescriptor = new JwtSecurityToken(
            issuer: Environment.GetEnvironmentVariable("JWT_ISSUER"),
            audience: Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(Environment.GetEnvironmentVariable("JWT_TOKEN_EXPIRY_MINUTES") ?? "60")),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }

    public async Task<string?> GetCurrentUserId() => await GetClaimValue(ClaimTypes.NameIdentifier);
    public async Task<string?> GetCurrentUserEmail() => await GetClaimValue(ClaimTypes.Name);
    public async Task<string?> GetCurrentUserRole() => await GetClaimValue(ClaimTypes.Role);
    private async Task<string?> GetClaimValue(string claimType) => httpContextAccessor.HttpContext?.User.FindFirst(claimType)?.Value;
}
