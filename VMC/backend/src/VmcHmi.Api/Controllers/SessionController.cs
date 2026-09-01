using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VmcHmi.Application;
using VmcHmi.Application.Commands;
using VmcHmi.Application.DTOs;
using VmcHmi.Application.Interfaces;
using VmcHmi.Application.Queries;
using VmcHmi.Domain.Entities;
using VmcHmi.Domain.Enums;
using VmcHmi.Domain.Exceptions;

namespace VmcHmi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SessionController : ControllerBase
{
    private readonly IRequestHandler<GetCurrentStateQuery, SessionStateResponse> _getStateHandler;
    private readonly IRequestHandler<ConfirmChecklistItemCommand> _confirmHandler;
    private readonly IRequestHandler<UnconfirmChecklistItemCommand> _unconfirmHandler;
    private readonly IRequestHandler<AdvanceStageCommand> _advanceHandler;
    private readonly IRequestHandler<StartOperationCommand> _startHandler;
    private readonly IRequestHandler<StopOperationCommand> _stopHandler;
    private readonly ICurrentUser _currentUser;
    private readonly IMachineSessionRepository _sessionRepo;

    public SessionController(
        IRequestHandler<GetCurrentStateQuery, SessionStateResponse> getStateHandler,
        IRequestHandler<ConfirmChecklistItemCommand> confirmHandler,
        IRequestHandler<UnconfirmChecklistItemCommand> unconfirmHandler,
        IRequestHandler<AdvanceStageCommand> advanceHandler,
        IRequestHandler<StartOperationCommand> startHandler,
        IRequestHandler<StopOperationCommand> stopHandler,
        ICurrentUser currentUser,
        IMachineSessionRepository sessionRepo)
    {
        _getStateHandler = getStateHandler;
        _confirmHandler = confirmHandler;
        _unconfirmHandler = unconfirmHandler;
        _advanceHandler = advanceHandler;
        _startHandler = startHandler;
        _stopHandler = stopHandler;
        _currentUser = currentUser;
        _sessionRepo = sessionRepo;
    }

    [HttpGet("current")]
    public async Task<ActionResult<SessionStateResponse>> GetCurrent()
    {
        var userId = _currentUser.UserId;
        if (userId == null)
            return Unauthorized();

        var session = await _sessionRepo.GetByUserIdAsync(userId.Value);
        if (session == null)
        {
            var newSession = CreateDefaultSession(userId.Value);
            await _sessionRepo.CreateAsync(newSession);
            await _sessionRepo.SaveChangesAsync();
            session = await _sessionRepo.GetByUserIdAsync(userId.Value);
        }

        if (session == null)
            return NotFound(new { message = "No active session found." });

        var response = await _getStateHandler.HandleAsync(new GetCurrentStateQuery { SessionId = session.Id });
        return Ok(response);
    }

    private static MachineSession CreateDefaultSession(Guid userId)
    {
        var session = new MachineSession(userId);

        var machineChecks = new[]
        {
            ("E-stop released and functional", 0),
            ("Emergency stop button tested", 1),
            ("Machine guard in place", 2),
            ("Coolant levels adequate", 3),
            ("Air pressure at operating level", 4),
            ("Spindle runout within tolerance", 5)
        };

        var tools = new[]
        {
            ("T01", "Face Mill 50mm"),
            ("T02", "End Mill 10mm (4-flute)"),
            ("T03", "Drill 8mm")
        };

        var workpieceChecks = new[]
        {
            ("Workpiece material verified (Aluminum 6061-T6)", 0),
            ("Workpiece orientation correct (datum face against fixed jaw)", 1),
            ("Pocket face up", 2),
            ("Torque clamp to 25 Nm", 3),
            ("Zero gap at datum face confirmed", 4)
        };

        var toolCheckLabels = new[]
        {
            ("T01 Face Mill 50mm installed and secured", 0),
            ("T02 End Mill 10mm (4-flute) installed and secured", 1),
            ("T03 Drill 8mm installed and secured", 2),
            ("All tool offsets set (G43 applied)", 3),
            ("Tool setter measurement verified", 4)
        };

        foreach (var (label, order) in machineChecks)
            session.AddChecklistItem(ChecklistStage.MachineChecks, label, order);

        foreach (var (code, desc) in tools)
            session.AddTool(code, desc);

        foreach (var (label, order) in toolCheckLabels)
            session.AddChecklistItem(ChecklistStage.Tools, label, order);

        foreach (var (label, order) in workpieceChecks)
            session.AddChecklistItem(ChecklistStage.Workpiece, label, order);

        return session;
    }

    [HttpPost("checklist/{itemId}/confirm")]
    public async Task<IActionResult> ConfirmChecklistItem(Guid itemId)
    {
        var userId = _currentUser.UserId;
        if (userId == null)
            return Unauthorized();

        var session = await _sessionRepo.GetByUserIdAsync(userId.Value);
        if (session == null)
            return NotFound(new { message = "Session not found." });

        try
        {
            await _confirmHandler.HandleAsync(new ConfirmChecklistItemCommand
            {
                SessionId = session.Id,
                ItemId = itemId
            });
            return Ok();
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("checklist/{itemId}/unconfirm")]
    public async Task<IActionResult> UnconfirmChecklistItem(Guid itemId)
    {
        var userId = _currentUser.UserId;
        if (userId == null)
            return Unauthorized();

        var session = await _sessionRepo.GetByUserIdAsync(userId.Value);
        if (session == null)
            return NotFound(new { message = "Session not found." });

        try
        {
            await _unconfirmHandler.HandleAsync(new UnconfirmChecklistItemCommand
            {
                SessionId = session.Id,
                ItemId = itemId
            });
            return Ok();
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("advance")]
    public async Task<IActionResult> AdvanceStage()
    {
        var userId = _currentUser.UserId;
        if (userId == null)
            return Unauthorized();

        var session = await _sessionRepo.GetByUserIdAsync(userId.Value);
        if (session == null)
            return NotFound(new { message = "Session not found." });

        try
        {
            await _advanceHandler.HandleAsync(new AdvanceStageCommand { SessionId = session.Id });
            return Ok();
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("operation/start")]
    public async Task<IActionResult> StartOperation()
    {
        var userId = _currentUser.UserId;
        if (userId == null)
            return Unauthorized();

        var session = await _sessionRepo.GetByUserIdAsync(userId.Value);
        if (session == null)
            return NotFound(new { message = "Session not found." });

        try
        {
            await _startHandler.HandleAsync(new StartOperationCommand { SessionId = session.Id });
            return Ok();
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("operation/stop")]
    public async Task<IActionResult> StopOperation()
    {
        var userId = _currentUser.UserId;
        if (userId == null)
            return Unauthorized();

        var session = await _sessionRepo.GetByUserIdAsync(userId.Value);
        if (session == null)
            return NotFound(new { message = "Session not found." });

        try
        {
            await _stopHandler.HandleAsync(new StopOperationCommand { SessionId = session.Id });
            return Ok();
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("reset")]
    public async Task<ActionResult<SessionStateResponse>> ResetSession()
    {
        var userId = _currentUser.UserId;
        if (userId == null)
            return Unauthorized();

        var session = await _sessionRepo.GetByUserIdAsync(userId.Value);
        if (session == null)
            return NotFound(new { message = "Session not found." });

        session.ResetWorkflow();
        await _sessionRepo.UpdateAsync(session);
        await _sessionRepo.SaveChangesAsync();

        var response = await _getStateHandler.HandleAsync(new GetCurrentStateQuery { SessionId = session.Id });
        return Ok(response);
    }
}
