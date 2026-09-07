namespace OnSteroidsApiUser.Application.Abstractions.Interfaces.IHelpers;

public interface IJwtTokenHelper
{
    (string Token, DateTime ExpiresAt) GenerateToken(Guid userId, string email);
}
