namespace OnSteroidsApiUser.Domain.Models.Request;

public class RequestParamDto
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public bool IsEnabled { get; set; } = true;
    public string ParamKey { get; set; } = string.Empty;
    public string ParamValue { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class RequestHeaderDto
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public bool IsEnabled { get; set; } = true;
    public string HeaderKey { get; set; } = string.Empty;
    public string HeaderValue { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class RequestFormDataDto
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public bool IsEnabled { get; set; } = true;
    public string FieldKey { get; set; } = string.Empty;
    public string FieldValue { get; set; } = string.Empty;
    public string FieldType { get; set; } = "text";
    public int SortOrder { get; set; }
}

public class RequestDto
{
    public Guid Id { get; set; }
    public Guid CapsuleId { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = "New Request";
    public string Url { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";

    public string PayloadType { get; set; } = "params";
    public string BodyType { get; set; } = "none";
    public string RawType { get; set; } = "JSON";
    public string? RawBody { get; set; }
    public string? RawBodyJson { get; set; }
    public string? RawBodyXml { get; set; }

    public string AuthType { get; set; } = "none";
    public string? AuthToken { get; set; }

    public string? PreRequestScript { get; set; }
    public string? PostResponseScript { get; set; }

    public string EncryptionAlgorithm { get; set; } = "none";
    public string? EncryptionKey { get; set; }
    public bool AutoEncryptBody { get; set; }
    public bool AutoEncryptHeaders { get; set; }
    public string? EncryptionChannel { get; set; }
    public string? EncryptedHeaders { get; set; }
    public string? EncryptedBodyPaths { get; set; }
    public string? EncryptionScript { get; set; }

    public bool FollowRedirects { get; set; } = true;
    public bool VerifySsl { get; set; } = true;
    public bool EnableCookies { get; set; } = true;
    public bool BypassCors { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<RequestParamDto> Params { get; set; } = [];
    public List<RequestHeaderDto> Headers { get; set; } = [];
    public List<RequestFormDataDto> FormData { get; set; } = [];
}

public class KeyValueItem
{
    public bool Enabled { get; set; } = true;
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class FormDataItem
{
    public bool Enabled { get; set; } = true;
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Type { get; set; } = "text";
    public int SortOrder { get; set; }
}

public class AuthStateDto
{
    public string Type { get; set; } = "none";
    public string? Token { get; set; }
}

public class ScriptsStateDto
{
    public string? PreRequest { get; set; }
    public string? PostResponse { get; set; }
}

public class EncryptionStateDto
{
    public string Algorithm { get; set; } = "none";
    public string? Key { get; set; }
    public bool AutoEncryptBody { get; set; }
    public bool AutoEncryptHeaders { get; set; }
    public string? ChannelName { get; set; }
    public List<string>? EncryptedHeaders { get; set; }
    public List<string>? EncryptedBodyPaths { get; set; }
    public string? Script { get; set; }
}

public class SettingsStateDto
{
    public bool FollowRedirects { get; set; } = true;
    public bool VerifySsl { get; set; } = true;
    public bool EnableCookies { get; set; } = true;
    public bool BypassCors { get; set; } = true;
}

public class SaveRequestStateRequest
{
    public string? Id { get; set; }
    public string? CapsuleId { get; set; }
    public string? CapsuleName { get; set; }
    public string Name { get; set; } = "New Request";
    public string Url { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";

    public string PayloadType { get; set; } = "params";
    public string BodyType { get; set; } = "none";
    public string RawType { get; set; } = "JSON";
    public string? RawBody { get; set; }
    public string? RawBodyJson { get; set; }
    public string? RawBodyXml { get; set; }

    public AuthStateDto? Auth { get; set; }
    public ScriptsStateDto? Scripts { get; set; }
    public EncryptionStateDto? Encryption { get; set; }
    public SettingsStateDto? Settings { get; set; }

    public List<KeyValueItem>? Params { get; set; }
    public List<KeyValueItem>? Headers { get; set; }
    public List<FormDataItem>? FormData { get; set; }
}
