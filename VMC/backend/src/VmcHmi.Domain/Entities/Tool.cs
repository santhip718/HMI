using VmcHmi.Domain.Enums;

namespace VmcHmi.Domain.Entities;

public class Tool
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Code { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string? RequiredByItemId { get; private set; }

    private Tool() { }

    public Tool(string code, string description)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new DomainException("Tool code cannot be empty.");
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("Tool description cannot be empty.");

        Code = code;
        Description = description;
    }
}
