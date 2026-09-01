using VmcHmi.Application.DTOs;
using VmcHmi.Application.Handlers;
using VmcHmi.Application.Queries;
using VmcHmi.Domain.Entities;
using VmcHmi.Domain.Enums;

namespace VmcHmi.Application.Tests;

public class GetCurrentStateHandlerTests
{
    // The checklist items exposed to the frontend must include their Stage so the
    // client can group items by stage without fragile label matching.
    [Fact]
    public async Task HandleAsync_IncludesStageForEachChecklistItem()
    {
        var userId = Guid.NewGuid();
        var session = new MachineSession(userId);
        session.AddChecklistItem(ChecklistStage.MachineChecks, "E-stop released and functional", 0);
        session.AddChecklistItem(ChecklistStage.Tools, "T01 Face Mill 50mm installed and secured", 0);
        session.AddChecklistItem(ChecklistStage.Workpiece, "Torque clamp to 25 Nm", 1);

        var repo = new InMemorySessionRepository();
        repo.Seed(session);
        var handler = new GetCurrentStateHandler(repo);

        SessionStateResponse response = await handler.HandleAsync(new GetCurrentStateQuery
        {
            SessionId = session.Id
        });

        Assert.Equal(3, response.ChecklistItems.Count);
        Assert.Contains(response.ChecklistItems, i => i.Stage == "MachineChecks");
        Assert.Contains(response.ChecklistItems, i => i.Stage == "Tools");
        Assert.Contains(response.ChecklistItems, i => i.Stage == "Workpiece");
        Assert.Equal(StageType.MachineChecks, response.CurrentStage);
        Assert.Empty(response.Tools); // no tools added in this fixture
    }

    [Fact]
    public async Task HandleAsync_WhenSessionMissing_ThrowsInvalidOperationException()
    {
        var repo = new InMemorySessionRepository();
        var handler = new GetCurrentStateHandler(repo);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(new GetCurrentStateQuery { SessionId = Guid.NewGuid() }));
    }
}
