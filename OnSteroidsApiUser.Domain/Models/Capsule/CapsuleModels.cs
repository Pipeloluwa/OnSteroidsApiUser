namespace OnSteroidsApiUser.Domain.Models.Capsule;

public class CapsuleDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AutoAuthEnabled { get; set; } = "off";
    public string? AutoAuthEndpointId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateCapsuleRequest
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AutoAuthEnabled { get; set; }
    public string? AutoAuthEndpointId { get; set; }
}

public class UpdateCapsuleRequest
{
    public string Name { get; set; } = string.Empty;
    public string? AutoAuthEnabled { get; set; }
    public string? AutoAuthEndpointId { get; set; }
}

public class SharedCapsuleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShareToken { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<Request.RequestDto> Requests { get; set; } = [];
}
