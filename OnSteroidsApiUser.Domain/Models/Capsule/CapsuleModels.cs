namespace OnSteroidsApiUser.Domain.Models.Capsule;

public class CapsuleDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateCapsuleRequest
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateCapsuleRequest
{
    public string Name { get; set; } = string.Empty;
}
