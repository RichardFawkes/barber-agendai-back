using BarbeariaSaaS.Domain.Entities;
using BarbeariaSaaS.Shared.DTOs.Response;

namespace BarbeariaSaaS.Application.Interfaces;

public interface IAuthenticationService
{
    Task<LoginResponseDto?> AuthenticateAsync(string email, string password);
    Task<string> GenerateJwtTokenAsync(User user);
    Task<string> GenerateRefreshTokenAsync();
    Task<bool> ValidateRefreshTokenAsync(string refreshToken);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
} 