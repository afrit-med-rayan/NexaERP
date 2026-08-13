namespace NexaERP.Application.DTOs.Auth;

public record UserProfileResponse(
    Guid Id,
    string FullName,
    string Email,
    IEnumerable<string> Roles,
    DateTime CreatedAt);
