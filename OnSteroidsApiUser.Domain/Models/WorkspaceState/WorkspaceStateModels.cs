namespace OnSteroidsApiUser.Domain.Models.WorkspaceState;

public class WorkspaceStateDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? ActiveCapsuleId { get; set; }
    public string? ActiveCapsuleName { get; set; }
    public string? ActiveTabId { get; set; }
    public string? OpenTabIds { get; set; }
    public string? TabStates { get; set; }
    public string? Responses { get; set; }
    public string? AutoAuthEnabled { get; set; } = "off";
    public string? AutoAuthEndpointId { get; set; }
    public bool AutoSaveEnabled { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UpsertWorkspaceStateRequest
{
    public string? ActiveCapsuleId { get; set; }
    public string? ActiveCapsuleName { get; set; }
    public string? ActiveTabId { get; set; }
    public string? OpenTabIds { get; set; }
    public string? TabStates { get; set; }
    public string? Responses { get; set; }
    public string? AutoAuthEnabled { get; set; }
    public string? AutoAuthEndpointId { get; set; }
    public bool AutoSaveEnabled { get; set; }
}
