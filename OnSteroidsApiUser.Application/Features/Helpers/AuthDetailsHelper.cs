using Microsoft.AspNetCore.Http;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IHelpers;
using System.Security.Claims;

namespace OnSteroidsApiUser.Application.Features.Helpers;

public class AuthDetailsHelper(IHttpContextAccessor httpContextAccessor) : IAuthDetailsHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public Guid UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var idClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? user?.FindFirst("sub")?.Value;

            return Guid.TryParse(idClaim, out var id) ? id : Guid.Empty;
        }
    }

    public string? Email =>
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

    public string? ClientIp =>
        _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

    public string RequestId =>
        _httpContextAccessor.HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString();
}
