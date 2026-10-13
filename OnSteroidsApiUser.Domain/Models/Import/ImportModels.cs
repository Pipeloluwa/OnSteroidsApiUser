using OnSteroidsApiUser.Domain.Models.Capsule;
using OnSteroidsApiUser.Domain.Models.Request;
using OnSteroidsApiUser.Domain.Models.Variable;

namespace OnSteroidsApiUser.Domain.Models.Import;

public class ImportCapsuleItemRequest
{
    public string? Id { get; set; }
    public string Name { get; set; } = "Imported Capsule";
    public string? AutoAuthEnabled { get; set; } = "off";
    public string? AutoAuthEndpointId { get; set; }
    public List<SaveRequestStateRequest> Requests { get; set; } = [];
    public List<SyncVariableItem>? Variables { get; set; }
}

public class ImportPayloadRequest
{
    public List<ImportCapsuleItemRequest> Capsules { get; set; } = [];
    public string? Name { get; set; }
    public string? AutoAuthEnabled { get; set; }
    public string? AutoAuthEndpointId { get; set; }
    public List<SaveRequestStateRequest>? Requests { get; set; }
    public List<SyncVariableItem>? Variables { get; set; }
}

public class ImportedCapsuleResultDto
{
    public CapsuleDto Capsule { get; set; } = default!;
    public List<RequestDto> Requests { get; set; } = [];
    public List<VariableDto> Variables { get; set; } = [];
}

public class ImportResultDto
{
    public int CapsulesCount { get; set; }
    public int RequestsCount { get; set; }
    public List<ImportedCapsuleResultDto> Capsules { get; set; } = [];
}
