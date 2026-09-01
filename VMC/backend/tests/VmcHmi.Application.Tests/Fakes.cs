using VmcHmi.Application;
using VmcHmi.Application.Interfaces;
using VmcHmi.Domain.Entities;
using VmcHmi.Domain.Exceptions;

namespace VmcHmi.Application.Tests;

public class InMemorySessionRepository : IMachineSessionRepository
{
    private readonly Dictionary<Guid, MachineSession> _sessions = new();

    public MachineSession? SavedSession { get; private set; }
    public int UpdateCount { get; private set; }
    public int SaveCount { get; private set; }

    public void Seed(MachineSession session) => _sessions[session.Id] = session;

    public Task<MachineSession?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _sessions.TryGetValue(id, out var session);
        return Task.FromResult(session);
    }

    public Task<MachineSession?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return Task.FromResult(_sessions.Values.FirstOrDefault(s => s.UserId == userId));
    }

    public Task<MachineSession> CreateAsync(MachineSession session, CancellationToken ct = default)
    {
        _sessions[session.Id] = session;
        return Task.FromResult(session);
    }

    public Task UpdateAsync(MachineSession session, CancellationToken ct = default)
    {
        SavedSession = session;
        UpdateCount++;
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        SaveCount++;
        return Task.CompletedTask;
    }
}

public class NullLogger<T> : IAppLogger<T>
{
    public void LogInformation(string message, params object[] args) { }
    public void LogWarning(string message, params object[] args) { }
    public void LogError(Exception ex, string message, params object[] args) { }
    public void LogError(string message, params object[] args) { }
}

public class FakePasswordHasher : IPasswordHasher
{
    private readonly string _knownPassword;
    public FakePasswordHasher(string knownPassword) => _knownPassword = knownPassword;

    public string Hash(string password) => "hashed:" + password;
    public bool Verify(string password, string hash) => password == _knownPassword;
}

public class FakeTokenService : ITokenService
{
    public string GeneratedToken { get; private set; } = string.Empty;
    public string GenerateToken(User user, string role)
    {
        GeneratedToken = $"tok-{user.Id}";
        return GeneratedToken;
    }
}

public class InMemoryUserRepository : IUserRepository
{
    private readonly Dictionary<string, User> _byUsername = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<Guid, User> _byId = new();

    public void Seed(User user)
    {
        _byUsername[user.Username] = user;
        _byId[user.Id] = user;
    }

    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
    {
        _byUsername.TryGetValue(username, out var u);
        return Task.FromResult(u);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _byId.TryGetValue(id, out var u);
        return Task.FromResult(u);
    }

    public Task<User> CreateAsync(User user, CancellationToken ct = default)
    {
        Seed(user);
        return Task.FromResult(user);
    }

    public Task SaveChangesAsync(CancellationToken ct = default) => Task.CompletedTask;
}

public static class SessionBuilder
{
    public static MachineSession FullyChecked(Guid userId)
    {
        var session = new MachineSession(userId);
        session.AddChecklistItem(Domain.Enums.ChecklistStage.MachineChecks, "E-stop released and functional", 0);

        foreach (var item in session.ChecklistItems.ToList())
        {
            session.ConfirmChecklistItem(item.Id);
        }

        return session;
    }
}
