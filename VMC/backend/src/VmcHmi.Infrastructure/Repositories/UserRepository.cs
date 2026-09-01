using Microsoft.EntityFrameworkCore;
using VmcHmi.Application.Interfaces;
using VmcHmi.Domain.Entities;

namespace VmcHmi.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly HmiDbContext _context;

    public UserRepository(HmiDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        var clean = username.Trim().ToLower();
        return await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == clean, ct);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<User> CreateAsync(User user, CancellationToken ct = default)
    {
        return (await _context.Users.AddAsync(user, ct)).Entity;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }
}
