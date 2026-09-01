using VmcHmi.Application.DTOs;
using VmcHmi.Domain.Entities;

namespace VmcHmi.Application.Interfaces;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
