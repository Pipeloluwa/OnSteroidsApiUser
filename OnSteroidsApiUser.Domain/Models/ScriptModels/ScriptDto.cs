namespace OnSteroidsApiUser.Domain.Models.ScriptModels;

public class ScriptDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public string? OriginalContent { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
