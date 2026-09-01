using VmcHmi.Application.Commands;
using VmcHmi.Application.Handlers;
using VmcHmi.Domain.Entities;
using VmcHmi.Domain.Enums;
using VmcHmi.Domain.Exceptions;

namespace VmcHmi.Application.Tests;

public class AdvanceStageHandlerTests
{
    // Gating is enforced at the Application boundary: advancing with incomplete
    // checklist items throws DomainException and nothing is persisted.
    [Fact]
    public async Task HandleAsync_WhenStageIncomplete_ThrowsDomainException()
    {
        var userId = Guid.NewGuid();
        var session = new MachineSession(userId);
        session.AddChecklistItem(ChecklistStage.MachineChecks, "E-stop released and functional", 0);
        session.AddChecklistItem(ChecklistStage.MachineChecks, "Machine guard in place", 1);

        var repo = new InMemorySessionRepository();
        repo.Seed(session);
        var handler = new AdvanceStageHandler(repo, new NullLogger<AdvanceStageHandler>());

        await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(new AdvanceStageCommand { SessionId = session.Id }));

        Assert.Equal(StageType.MachineChecks, session.CurrentStage);
        Assert.Equal(0, repo.SaveCount);
    }

    // When the active stage is fully confirmed, advancing persists the new stage.
    [Fact]
    public async Task HandleAsync_WhenStageComplete_AdvancesAndPersists()
    {
        var userId = Guid.NewGuid();
        var session = new MachineSession(userId);
        session.AddChecklistItem(ChecklistStage.MachineChecks, "E-stop released and functional", 0);
        foreach (var item in session.ChecklistItems.ToList())
        {
            session.ConfirmChecklistItem(item.Id);
        }

        var repo = new InMemorySessionRepository();
        repo.Seed(session);
        var handler = new AdvanceStageHandler(repo, new NullLogger<AdvanceStageHandler>());

        await handler.HandleAsync(new AdvanceStageCommand { SessionId = session.Id });

        Assert.Equal(StageType.Tools, session.CurrentStage);
        Assert.Equal(1, repo.UpdateCount);
        Assert.Equal(1, repo.SaveCount);
        Assert.Same(session, repo.SavedSession);
    }

    // A missing session is reported, not silently allowed.
    [Fact]
    public async Task HandleAsync_WhenSessionMissing_ThrowsDomainException()
    {
        var repo = new InMemorySessionRepository();
        var handler = new AdvanceStageHandler(repo, new NullLogger<AdvanceStageHandler>());

        await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(new AdvanceStageCommand { SessionId = Guid.NewGuid() }));
    }
}
