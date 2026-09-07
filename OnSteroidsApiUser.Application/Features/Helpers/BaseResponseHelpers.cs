using OnSteroidsApiUser.Domain.Models.Common.BaseModels.Responses;
using System.Net;

namespace OnSteroidsApiUser.Application.Features.Helpers;

public static class BaseResponseHelpers
{
    public static readonly Tuple<int, string, string, string> SuccessData =
        new((int)HttpStatusCode.OK, "Operation Successful", "01", "Request is successful");

    public static readonly Tuple<int, string, string, string> ValidationSyntaxErrorData =
        new((int)HttpStatusCode.BadRequest, "Validation Error", "03", "Invalid request, please check your request");

    public static readonly Tuple<int, string, string, string> ValidationSemanticErrorData =
        new((int)HttpStatusCode.UnprocessableEntity, "Validation Error", "04", "Invalid request, please check your request");

    public static readonly Tuple<int, string, string, string> UnauthorizedErrorData =
        new((int)HttpStatusCode.Unauthorized, "Unauthorized", "05", "User is not authorized");

    public static readonly Tuple<int, string, string, string> NotFoundErrorData =
        new((int)HttpStatusCode.NotFound, "Not Found", "06", "Resource not found");

    public static readonly Tuple<int, string, string, string> ServerErrorData =
        new((int)HttpStatusCode.InternalServerError, "Error Occurred", "08", "Something went wrong with your request, please try again later");

    public static (int StatusCode, BaseSuccessResponse<T> Data) ReturnSuccess<T>(string? message, T? data)
    {
        return (
            SuccessData.Item1,
            new BaseSuccessResponse<T>(SuccessData.Item2, SuccessData.Item3, message ?? SuccessData.Item4, data)
        );
    }

    public static (int StatusCode, BaseErrorResponse Error) ReturnValidationSyntaxError(string? message, IEnumerable<string>? errors)
    {
        return (
            ValidationSyntaxErrorData.Item1,
            new BaseErrorResponse(ValidationSyntaxErrorData.Item2, ValidationSyntaxErrorData.Item3, message ?? ValidationSyntaxErrorData.Item4, errors)
        );
    }

    public static (int StatusCode, BaseErrorResponse Error) ReturnValidationSemanticErrorData(string? message, IEnumerable<string>? errors)
    {
        return (
            ValidationSemanticErrorData.Item1,
            new BaseErrorResponse(ValidationSemanticErrorData.Item2, ValidationSemanticErrorData.Item3, message ?? ValidationSemanticErrorData.Item4, errors)
        );
    }

    public static (int StatusCode, BaseErrorResponse Error) ReturnUnauthorizedError(string? message, IEnumerable<string>? errors)
    {
        return (
            UnauthorizedErrorData.Item1,
            new BaseErrorResponse(UnauthorizedErrorData.Item2, UnauthorizedErrorData.Item3, message ?? UnauthorizedErrorData.Item4, errors)
        );
    }

    public static (int StatusCode, BaseErrorResponse Error) ReturnNotFoundError(string? message, IEnumerable<string>? errors)
    {
        return (
            NotFoundErrorData.Item1,
            new BaseErrorResponse(NotFoundErrorData.Item2, NotFoundErrorData.Item3, message ?? NotFoundErrorData.Item4, errors)
        );
    }

    public static (int StatusCode, BaseErrorResponse Error) ReturnServerErrorData(string? message, IEnumerable<string>? errors)
    {
        return (
            ServerErrorData.Item1,
            new BaseErrorResponse(ServerErrorData.Item2, ServerErrorData.Item3, message ?? ServerErrorData.Item4, errors)
        );
    }
}
