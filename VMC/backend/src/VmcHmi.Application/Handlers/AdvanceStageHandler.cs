using VmcHmi.Application.Commands;
using VmcHmi.Application.Interfaces;
using VmcHmi.Domain.Exceptions;

namespace VmcHmi.Application.Handlers;

public class AdvanceStageHandler : IRequestHandler<AdvanceStageCommand>
{
    private readonly IMachineSessionRepository _sessionRepo;
    private readonly IAppLogger<AdvanceStageHandler> _logger;

    public AdvanceStageHandler(IMachineSessionRepository sessionRepo, IAppLogger<AdvanceStageHandler> logger)
    {
        _sessionRepo = sessionRepo;
        _logger = logger;
    }

    public async Task HandleAsync(AdvanceStageCommand request, CancellationToken ct = default)
    {
        var session = await _sessionRepo.GetByIdAsync(request.SessionId, ct);
        if (session == null)
            throw new DomainException("Session not found.");

        session.AdvanceStage();
        _logger.LogInformation("Advanced session {SessionId} to stage {Stage}", request.SessionId, session.CurrentStage);

        await _sessionRepo.UpdateAsync(session, ct);
        await _sessionRepo.SaveChangesAsync(ct);
    }
}
