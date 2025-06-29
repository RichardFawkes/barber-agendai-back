using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using BarbeariaSaaS.Application.Interfaces;
using BarbeariaSaaS.Domain.Entities;
using BarbeariaSaaS.Shared.DTOs.Response;

namespace BarbeariaSaaS.Infrastructure.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly JwtTokenService _jwtTokenService;
    private readonly IMapper _mapper;

    public AuthenticationService(IUnitOfWork unitOfWork, JwtTokenService jwtTokenService, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _mapper = mapper;
    }

    public async Task<LoginResponseDto?> AuthenticateAsync(string email, string password)
    {
        // Find user by email
        var user = await _unitOfWork.Users.GetByEmailAsync(email);
        if (user == null || !user.IsActive)
        {
            return null;
        }

        // Verify password
        if (!VerifyPassword(password, user.PasswordHash))
        {
            return null;
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        _unitOfWork.Users.Update(user);

        // Generate tokens
        var token = await GenerateJwtTokenAsync(user);
        var refreshToken = await GenerateRefreshTokenAsync();

        // Get tenant information if user has one
        TenantDto? tenantDto = null;
        if (user.TenantId.HasValue && user.Tenant != null)
        {
            tenantDto = _mapper.Map<TenantDto>(user.Tenant);
        }

        await _unitOfWork.SaveChangesAsync();

        return new LoginResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            User = _mapper.Map<UserDto>(user),
            Tenant = tenantDto,
            ExpiresAt = _jwtTokenService.GetTokenExpiration()
        };
    }

    public Task<string> GenerateJwtTokenAsync(User user)
    {
        var token = _jwtTokenService.GenerateToken(user);
        return Task.FromResult(token);
    }

    public Task<string> GenerateRefreshTokenAsync()
    {
        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        return Task.FromResult(refreshToken);
    }

    public Task<bool> ValidateRefreshTokenAsync(string refreshToken)
    {
        // TODO: Implement refresh token validation with database storage
        // For now, return true for valid base64 strings
        try
        {
            Convert.FromBase64String(refreshToken);
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var salt = GenerateSalt();
        var saltedPassword = password + salt;
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
        var hash = Convert.ToBase64String(hashedBytes);
        return $"{salt}:{hash}";
    }

    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            var parts = hash.Split(':');
            if (parts.Length != 2)
                return false;

            var salt = parts[0];
            var storedHash = parts[1];

            using var sha256 = SHA256.Create();
            var saltedPassword = password + salt;
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
            var computedHash = Convert.ToBase64String(hashedBytes);

            return storedHash == computedHash;
        }
        catch
        {
            return false;
        }
    }

    private static string GenerateSalt()
    {
        var saltBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(saltBytes);
        return Convert.ToBase64String(saltBytes);
    }
} 