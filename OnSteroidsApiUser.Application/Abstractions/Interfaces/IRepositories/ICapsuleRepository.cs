using OnSteroidsApiUser.Domain.Models.Capsule;

namespace OnSteroidsApiUser.Application.Abstractions.Interfaces.IRepositories;

public interface ICapsuleRepository
{
    Task<CapsuleDto?> CreateAsync(Guid userId, string name);
    Task<IEnumerable<CapsuleDto>> GetAllByUserAsync(Guid userId);
    Task<CapsuleDto?> GetByIdAsync(Guid id, Guid userId);
    Task<CapsuleDto?> UpdateAsync(Guid id, Guid userId, string name);
    Task DeleteAsync(Guid id, Guid userId);
}
