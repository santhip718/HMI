using VmcHmi.Application.Commands;
using VmcHmi.Application.Interfaces;
using VmcHmi.Domain.Exceptions;

namespace VmcHmi.Application.Handlers;

public class UnconfirmChecklistItemHandler : IRequestHandler<UnconfirmChecklistItemCommand>
{
    private readonly IMachineSessionRepository _sessionRepo;
    private readonly IAppLogger<UnconfirmChecklistItemHandler> _logger;

    public UnconfirmChecklistItemHandler(IMachineSessionRepository sessionRepo, IAppLogger<UnconfirmChecklistItemHandler> logger)
    {
        _sessionRepo = sessionRepo;
        _logger = logger;
    }

    public async Task HandleAsync(UnconfirmChecklistItemCommand request, CancellationToken ct = default)
    {
        var session = await _sessionRepo.GetByIdAsync(request.SessionId, ct);
        if (session == null)
            throw new DomainException("Session not found.");

        session.UnconfirmChecklistItem(request.ItemId);
        _logger.LogInformation("Unconfirmed checklist item {ItemId} for session {SessionId}", request.ItemId, request.SessionId);

        await _sessionRepo.UpdateAsync(session, ct);
        await _sessionRepo.SaveChangesAsync(ct);
    }
}
