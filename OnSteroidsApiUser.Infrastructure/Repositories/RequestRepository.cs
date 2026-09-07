using Dapper;
using Microsoft.Extensions.Logging;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IDapper;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IRepositories;
using OnSteroidsApiUser.Domain.Models.Request;

namespace OnSteroidsApiUser.Infrastructure.Repositories;

public class RequestRepository(
    IDapperConnection dapperConnection,
    ILogger<RequestRepository> logger
) : IRequestRepository
{
    private readonly IDapperConnection _dapper = dapperConnection;
    private readonly ILogger<RequestRepository> _logger = logger;

    public async Task<RequestDto?> CreateAsync(RequestDto req)
    {
        _logger.LogInformation("Creating request {Name} in capsule {CapsuleId}", req.Name, req.CapsuleId);
        var p = new DynamicParameters();
        p.Add("@CapsuleId", req.CapsuleId);
        p.Add("@UserId", req.UserId);
        p.Add("@Name", req.Name);
        p.Add("@Url", req.Url);
        p.Add("@Method", req.Method);
        p.Add("@PayloadType", req.PayloadType);
        p.Add("@BodyType", req.BodyType);
        p.Add("@RawType", req.RawType);
        p.Add("@RawBody", req.RawBody);
        p.Add("@RawBodyJson", req.RawBodyJson);
        p.Add("@RawBodyXml", req.RawBodyXml);
        p.Add("@AuthType", req.AuthType);
        p.Add("@AuthToken", req.AuthToken);
        p.Add("@PreRequestScript", req.PreRequestScript);
        p.Add("@PostResponseScript", req.PostResponseScript);
        p.Add("@EncryptionAlgorithm", req.EncryptionAlgorithm);
        p.Add("@EncryptionKey", req.EncryptionKey);
        p.Add("@AutoEncryptBody", req.AutoEncryptBody);
        p.Add("@AutoEncryptHeaders", req.AutoEncryptHeaders);
        p.Add("@EncryptionChannel", req.EncryptionChannel);
        p.Add("@EncryptedHeaders", req.EncryptedHeaders);
        p.Add("@EncryptedBodyPaths", req.EncryptedBodyPaths);
        p.Add("@EncryptionScript", req.EncryptionScript);
        p.Add("@FollowRedirects", req.FollowRedirects);
        p.Add("@VerifySsl", req.VerifySsl);
        p.Add("@EnableCookies", req.EnableCookies);
        p.Add("@BypassCors", req.BypassCors);

        return await _dapper.Query<RequestDto>(p, "dbo.spRequest_Create");
    }

    public async Task<RequestDto?> UpdateAsync(RequestDto req)
    {
        _logger.LogInformation("Updating request {Id} for user {UserId}", req.Id, req.UserId);
        var p = new DynamicParameters();
        p.Add("@Id", req.Id);
        p.Add("@UserId", req.UserId);
        p.Add("@Name", req.Name);
        p.Add("@Url", req.Url);
        p.Add("@Method", req.Method);
        p.Add("@PayloadType", req.PayloadType);
        p.Add("@BodyType", req.BodyType);
        p.Add("@RawType", req.RawType);
        p.Add("@RawBody", req.RawBody);
        p.Add("@RawBodyJson", req.RawBodyJson);
        p.Add("@RawBodyXml", req.RawBodyXml);
        p.Add("@AuthType", req.AuthType);
        p.Add("@AuthToken", req.AuthToken);
        p.Add("@PreRequestScript", req.PreRequestScript);
        p.Add("@PostResponseScript", req.PostResponseScript);
        p.Add("@EncryptionAlgorithm", req.EncryptionAlgorithm);
        p.Add("@EncryptionKey", req.EncryptionKey);
        p.Add("@AutoEncryptBody", req.AutoEncryptBody);
        p.Add("@AutoEncryptHeaders", req.AutoEncryptHeaders);
        p.Add("@EncryptionChannel", req.EncryptionChannel);
        p.Add("@EncryptedHeaders", req.EncryptedHeaders);
        p.Add("@EncryptedBodyPaths", req.EncryptedBodyPaths);
        p.Add("@EncryptionScript", req.EncryptionScript);
        p.Add("@FollowRedirects", req.FollowRedirects);
        p.Add("@VerifySsl", req.VerifySsl);
        p.Add("@EnableCookies", req.EnableCookies);
        p.Add("@BypassCors", req.BypassCors);

        return await _dapper.Query<RequestDto>(p, "dbo.spRequest_Update");
    }

    public async Task<RequestDto?> GetByIdAsync(Guid id, Guid userId)
    {
        var p = new DynamicParameters();
        p.Add("@Id", id);
        p.Add("@UserId", userId);
        return await _dapper.Query<RequestDto>(p, "dbo.spRequest_GetById");
    }

    public async Task<IEnumerable<RequestDto>> GetAllByCapsuleAsync(Guid capsuleId, Guid userId)
    {
        var p = new DynamicParameters();
        p.Add("@CapsuleId", capsuleId);
        p.Add("@UserId", userId);
        return await _dapper.QueryAll<RequestDto>(p, "dbo.spRequest_GetAllByCapsule");
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var p = new DynamicParameters();
        p.Add("@Id", id);
        p.Add("@UserId", userId);
        await _dapper.Execute(p, "dbo.spRequest_Delete");
    }

    public async Task<RequestDto?> DuplicateAsync(Guid sourceId, Guid userId)
    {
        var p = new DynamicParameters();
        p.Add("@SourceId", sourceId);
        p.Add("@UserId", userId);
        return await _dapper.Query<RequestDto>(p, "dbo.spRequest_Duplicate");
    }

    public async Task<IEnumerable<RequestParamDto>> SyncParamsAsync(Guid requestId, string jsonData)
    {
        var p = new DynamicParameters();
        p.Add("@RequestId", requestId);
        p.Add("@JsonData", jsonData);
        return await _dapper.QueryAll<RequestParamDto>(p, "dbo.spRequestParam_Sync");
    }

    public async Task<IEnumerable<RequestParamDto>> GetParamsByRequestAsync(Guid requestId)
    {
        var p = new DynamicParameters();
        p.Add("@RequestId", requestId);
        return await _dapper.QueryAll<RequestParamDto>(p, "dbo.spRequestParam_GetByRequest");
    }

    public async Task<IEnumerable<RequestHeaderDto>> SyncHeadersAsync(Guid requestId, string jsonData)
    {
        var p = new DynamicParameters();
        p.Add("@RequestId", requestId);
        p.Add("@JsonData", jsonData);
        return await _dapper.QueryAll<RequestHeaderDto>(p, "dbo.spRequestHeader_Sync");
    }

    public async Task<IEnumerable<RequestHeaderDto>> GetHeadersByRequestAsync(Guid requestId)
    {
        var p = new DynamicParameters();
        p.Add("@RequestId", requestId);
        return await _dapper.QueryAll<RequestHeaderDto>(p, "dbo.spRequestHeader_GetByRequest");
    }

    public async Task<IEnumerable<RequestFormDataDto>> SyncFormDataAsync(Guid requestId, string jsonData)
    {
        var p = new DynamicParameters();
        p.Add("@RequestId", requestId);
        p.Add("@JsonData", jsonData);
        return await _dapper.QueryAll<RequestFormDataDto>(p, "dbo.spRequestFormData_Sync");
    }

    public async Task<IEnumerable<RequestFormDataDto>> GetFormDataByRequestAsync(Guid requestId)
    {
        var p = new DynamicParameters();
        p.Add("@RequestId", requestId);
        return await _dapper.QueryAll<RequestFormDataDto>(p, "dbo.spRequestFormData_GetByRequest");
    }
}
