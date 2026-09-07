using Microsoft.AspNetCore.Mvc;

namespace OnSteroidsApiUser.Application.Features.Helpers;

public static class MiddlewareHelpers
{
    public static IActionResult ReturnValidationResponse(ActionContext context)
    {
        var errors = context.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .SelectMany(e => e.Value!.Errors.Select(x => $"{e.Key}: {x.ErrorMessage}"))
            .ToList();

        (int status, var response) = BaseResponseHelpers.ReturnValidationSyntaxError("Validation failed", errors);
        return new ObjectResult(response) { StatusCode = status };
    }
}
