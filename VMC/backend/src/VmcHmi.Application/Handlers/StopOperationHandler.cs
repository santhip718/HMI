using VmcHmi.Application.Commands;
using VmcHmi.Application.Interfaces;
using VmcHmi.Domain.Exceptions;

namespace VmcHmi.Application.Handlers;

public class StopOperationHandler : IRequestHandler<StopOperationCommand>
{
    private readonly IMachineSessionRepository _sessionRepo;
    private readonly IAppLogger<StopOperationHandler> _logger;

    public StopOperationHandler(IMachineSessionRepository sessionRepo, IAppLogger<StopOperationHandler> logger)
    {
        _sessionRepo = sessionRepo;
        _logger = logger;
    }

    public async Task HandleAsync(StopOperationCommand request, CancellationToken ct = default)
    {
        var session = await _sessionRepo.GetByIdAsync(request.SessionId, ct);
        if (session == null)
            throw new DomainException("Session not found.");

        session.StopOperation();
        _logger.LogInformation("Stopped operation for session {SessionId}", request.SessionId);

        await _sessionRepo.UpdateAsync(session, ct);
        await _sessionRepo.SaveChangesAsync(ct);
    }
}
