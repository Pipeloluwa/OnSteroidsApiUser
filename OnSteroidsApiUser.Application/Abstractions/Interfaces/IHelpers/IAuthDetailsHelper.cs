namespace OnSteroidsApiUser.Application.Abstractions.Interfaces.IHelpers;

public interface IAuthDetailsHelper
{
    Guid UserId { get; }
    string? Email { get; }
    string? ClientIp { get; }
    string RequestId { get; }
}
