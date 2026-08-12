namespace NexaERP.Domain.Exceptions;

/// <summary>
/// Thrown when a business rule is violated. Maps to HTTP 422 Unprocessable Entity.
/// </summary>
public class BusinessException : Exception
{
    public BusinessException(string message) : base(message) { }
}
