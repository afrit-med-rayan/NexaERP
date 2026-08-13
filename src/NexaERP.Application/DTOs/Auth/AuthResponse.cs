namespace NexaERP.Application.DTOs.Auth;

public record AuthResponse(
    string Token,
    string FullName,
    string Email,
    IEnumerable<string> Roles,
    DateTime ExpiresAt);
