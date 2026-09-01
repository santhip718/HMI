using VmcHmi.Application.DTOs;
using VmcHmi.Application.Interfaces;
using VmcHmi.Application.Queries;
using VmcHmi.Domain.Entities;

namespace VmcHmi.Application.Handlers;

public class LoginHandler : IRequestHandler<LoginQuery, LoginResponse>
{
    private readonly IUserRepository _userRepo;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IAppLogger<LoginHandler> _logger;

    public LoginHandler(
        IUserRepository userRepo,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IAppLogger<LoginHandler> logger)
    {
        _userRepo = userRepo;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<LoginResponse> HandleAsync(LoginQuery request, CancellationToken ct = default)
    {
        var cleanUsername = request.Username.Trim();
        var user = await _userRepo.GetByUsernameAsync(cleanUsername, ct);

        // Self-healing / On-demand fallback if initial seed was delayed
        if (user == null)
        {
            if (cleanUsername.Equals("operator", StringComparison.OrdinalIgnoreCase) && request.Password == "Operator@123")
            {
                user = new User("operator", _passwordHasher.Hash("Operator@123"));
                await _userRepo.CreateAsync(user, ct);
                await _userRepo.SaveChangesAsync(ct);
            }
            else if (cleanUsername.Equals("Hr123@gmail.com", StringComparison.OrdinalIgnoreCase) && request.Password == "Hr@123")
            {
                user = new User("Hr123@gmail.com", _passwordHasher.Hash("Hr@123"));
                await _userRepo.CreateAsync(user, ct);
                await _userRepo.SaveChangesAsync(ct);
            }
        }

        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed for username: {Username}", request.Username);
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        _logger.LogInformation("Login succeeded for username: {Username}", request.Username);
        var token = _tokenService.GenerateToken(user, "Operator");

        return new LoginResponse
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(8)
        };
    }
}
