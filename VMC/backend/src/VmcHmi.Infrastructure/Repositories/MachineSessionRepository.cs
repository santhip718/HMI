using Microsoft.EntityFrameworkCore;
using VmcHmi.Application.Interfaces;
using VmcHmi.Domain.Entities;

namespace VmcHmi.Infrastructure.Repositories;

public class MachineSessionRepository : IMachineSessionRepository
{
    private readonly HmiDbContext _context;

    public MachineSessionRepository(HmiDbContext context)
    {
        _context = context;
    }

    public async Task<MachineSession?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.MachineSessions
            .Include(s => s.ChecklistItems)
            .Include(s => s.Tools)
            .Include(s => s.OperationRun)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<MachineSession?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.MachineSessions
            .Include(s => s.ChecklistItems)
            .Include(s => s.Tools)
            .Include(s => s.OperationRun)
            .FirstOrDefaultAsync(s => s.UserId == userId, ct);
    }

    public async Task<MachineSession> CreateAsync(MachineSession session, CancellationToken ct = default)
    {
        return (await _context.MachineSessions.AddAsync(session, ct)).Entity;
    }

    public async Task UpdateAsync(MachineSession session, CancellationToken ct = default)
    {
        _context.MachineSessions.Update(session);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
