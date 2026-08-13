using NexaERP.Application.DTOs.Auth;

namespace NexaERP.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<UserProfileResponse> GetProfileAsync(Guid userId);
}
