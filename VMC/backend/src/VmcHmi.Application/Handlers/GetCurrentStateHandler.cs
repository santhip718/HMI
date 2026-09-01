using VmcHmi.Application.DTOs;
using VmcHmi.Application.Interfaces;
using VmcHmi.Application.Queries;

namespace VmcHmi.Application.Handlers;

public class GetCurrentStateHandler : IRequestHandler<GetCurrentStateQuery, SessionStateResponse>
{
    private readonly IMachineSessionRepository _sessionRepo;

    public GetCurrentStateHandler(IMachineSessionRepository sessionRepo)
    {
        _sessionRepo = sessionRepo;
    }

    public async Task<SessionStateResponse> HandleAsync(GetCurrentStateQuery request, CancellationToken ct = default)
    {
        var session = await _sessionRepo.GetByIdAsync(request.SessionId, ct);
        if (session == null)
            throw new InvalidOperationException("Session not found.");

        return new SessionStateResponse
        {
            SessionId = session.Id,
            CurrentStage = session.CurrentStage,
            OperationStatus = session.OperationStatus,
            ChecklistItems = session.ChecklistItems
                .OrderBy(i => i.SortOrder)
                .Select(i => new ChecklistItemDto
                {
                    Id = i.Id,
                    Stage = i.Stage.ToString(),
                    Label = i.Label,
                    SortOrder = i.SortOrder,
                    IsConfirmed = i.IsConfirmed,
                    ConfirmedAt = i.ConfirmedAt
                }).ToList(),
            Tools = session.Tools.Select(t => new ToolDto
            {
                Id = t.Id,
                Code = t.Code,
                Description = t.Description
            }).ToList(),
            OperationRun = session.OperationRun == null ? null : new OperationRunDto
            {
                Id = session.OperationRun.Id,
                Status = session.OperationRun.Status,
                StartedAt = session.OperationRun.StartedAt,
                StoppedAt = session.OperationRun.StoppedAt
            }
        };
    }
}
