using VmcHmi.Application.Commands;
using VmcHmi.Application.Interfaces;
using VmcHmi.Domain.Exceptions;

namespace VmcHmi.Application.Handlers;

public class ConfirmChecklistItemHandler : IRequestHandler<ConfirmChecklistItemCommand>
{
    private readonly IMachineSessionRepository _sessionRepo;
    private readonly IAppLogger<ConfirmChecklistItemHandler> _logger;

    public ConfirmChecklistItemHandler(IMachineSessionRepository sessionRepo, IAppLogger<ConfirmChecklistItemHandler> logger)
    {
        _sessionRepo = sessionRepo;
        _logger = logger;
    }

    public async Task HandleAsync(ConfirmChecklistItemCommand request, CancellationToken ct = default)
    {
        var session = await _sessionRepo.GetByIdAsync(request.SessionId, ct);
        if (session == null)
            throw new DomainException("Session not found.");

        session.ConfirmChecklistItem(request.ItemId);
        _logger.LogInformation("Confirmed checklist item {ItemId} for session {SessionId}", request.ItemId, request.SessionId);

        await _sessionRepo.UpdateAsync(session, ct);
        await _sessionRepo.SaveChangesAsync(ct);
    }
}
