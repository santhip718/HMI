using VmcHmi.Application.Commands;
using VmcHmi.Application.Interfaces;
using VmcHmi.Domain.Exceptions;

namespace VmcHmi.Application.Handlers;

public class StartOperationHandler : IRequestHandler<StartOperationCommand>
{
    private readonly IMachineSessionRepository _sessionRepo;
    private readonly IAppLogger<StartOperationHandler> _logger;

    public StartOperationHandler(IMachineSessionRepository sessionRepo, IAppLogger<StartOperationHandler> logger)
    {
        _sessionRepo = sessionRepo;
        _logger = logger;
    }

    public async Task HandleAsync(StartOperationCommand request, CancellationToken ct = default)
    {
        var session = await _sessionRepo.GetByIdAsync(request.SessionId, ct);
        if (session == null)
            throw new DomainException("Session not found.");

        session.StartOperation();
        _logger.LogInformation("Started operation for session {SessionId}", request.SessionId);

        await _sessionRepo.UpdateAsync(session, ct);
        await _sessionRepo.SaveChangesAsync(ct);
    }
}
