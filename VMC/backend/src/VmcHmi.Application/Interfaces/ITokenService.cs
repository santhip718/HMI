using VmcHmi.Domain.Entities;

namespace VmcHmi.Application.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user, string role);
}
