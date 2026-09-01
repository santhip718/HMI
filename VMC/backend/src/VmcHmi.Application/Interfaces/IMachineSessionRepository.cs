using VmcHmi.Domain.Entities;

namespace VmcHmi.Application.Interfaces;

public interface IMachineSessionRepository
{
    Task<MachineSession?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<MachineSession?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<MachineSession> CreateAsync(MachineSession session, CancellationToken ct = default);
    Task UpdateAsync(MachineSession session, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
