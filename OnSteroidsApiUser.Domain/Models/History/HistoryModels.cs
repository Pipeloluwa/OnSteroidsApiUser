namespace OnSteroidsApiUser.Domain.Models.History;

public class RequestHistoryDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? RequestSnapshot { get; set; }
    public int? ResponseStatus { get; set; }
    public int? ResponseTime { get; set; }
    public long? ResponseSize { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateHistoryRequest
{
    public string Method { get; set; } = "GET";
    public string Url { get; set; } = string.Empty;
    public string? RequestSnapshot { get; set; }
    public int? ResponseStatus { get; set; }
    public int? ResponseTime { get; set; }
    public long? ResponseSize { get; set; }
}
