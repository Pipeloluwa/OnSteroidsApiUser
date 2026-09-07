using Microsoft.Extensions.Logging;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IRepositories;
using OnSteroidsApiUser.Application.Abstractions.Interfaces.IServices;
using OnSteroidsApiUser.Application.Features.Helpers;
using OnSteroidsApiUser.Domain.Models.History;

namespace OnSteroidsApiUser.Application.Services;

public class RequestHistoryService(
    IRequestHistoryRepository historyRepository,
    ILogger<RequestHistoryService> logger
) : IRequestHistoryService
{
    private readonly IRequestHistoryRepository _historyRepo = historyRepository;
    private readonly ILogger<RequestHistoryService> _logger = logger;

    public async Task<(int StatusCode, object Response)> CreateHistoryAsync(CreateHistoryRequest request, Guid userId)
    {
        var item = await _historyRepo.CreateAsync(
            userId,
            request.Method,
            request.Url,
            request.RequestSnapshot,
            request.ResponseStatus,
            request.ResponseTime,
            request.ResponseSize
        );

        return BaseResponseHelpers.ReturnSuccess("History recorded", item);
    }

    public async Task<(int StatusCode, object Response)> GetAllByUserAsync(Guid userId, int limit = 100)
    {
        var items = await _historyRepo.GetAllByUserAsync(userId, limit);
        return BaseResponseHelpers.ReturnSuccess("History retrieved", items);
    }

    public async Task<(int StatusCode, object Response)> DeleteAsync(Guid id, Guid userId)
    {
        await _historyRepo.DeleteAsync(id, userId);
        return BaseResponseHelpers.ReturnSuccess<object>("History entry deleted", null);
    }

    public async Task<(int StatusCode, object Response)> ClearAsync(Guid userId)
    {
        await _historyRepo.ClearAsync(userId);
        return BaseResponseHelpers.ReturnSuccess<object>("History cleared", null);
    }
}
