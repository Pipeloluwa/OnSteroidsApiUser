namespace OnSteroidsApiUser.Domain.Models.Common.BaseModels.Responses;

public class BaseSuccessResponse<T>(string title, string responseCode, string message, T? data)
{
    public string? title { get; set; } = title;
    public string? responseCode { get; set; } = responseCode;
    public string? message { get; set; } = message;
    public T? data { get; set; } = data;
}

public class BaseErrorResponse(string title, string responseCode, string message, IEnumerable<string>? errors)
{
    public string? title { get; set; } = title;
    public string? responseCode { get; set; } = responseCode;
    public string? message { get; set; } = message;
    public IEnumerable<string>? errors { get; set; } = errors;
}
