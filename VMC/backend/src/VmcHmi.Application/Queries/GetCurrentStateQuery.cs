using VmcHmi.Application.DTOs;

namespace VmcHmi.Application.Queries;

public class GetCurrentStateQuery
{
    public Guid SessionId { get; set; }
}
