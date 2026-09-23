namespace OnSteroidsApiUser.Domain.Models.Variable;

public class VariableDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? CapsuleId { get; set; }
    public string VariableKey { get; set; } = string.Empty;
    public string VariableValue { get; set; } = string.Empty;
    public string Type { get; set; } = "global";
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class SyncVariableItem
{
    public string? Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = "global";
    public bool Enabled { get; set; } = true;
}

public class SyncVariablesRequest
{
    public Guid? CapsuleId { get; set; }
    public List<SyncVariableItem> Variables { get; set; } = [];
}
