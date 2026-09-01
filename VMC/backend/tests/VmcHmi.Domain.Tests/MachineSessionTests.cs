using VmcHmi.Domain.Entities;
using VmcHmi.Domain.Enums;
using VmcHmi.Domain.Exceptions;

namespace VmcHmi.Domain.Tests;

public class MachineSessionTests
{
    private static readonly Guid UserId = Guid.NewGuid();

    private static MachineSession CreateSession()
    {
        var session = new MachineSession(UserId);

        session.AddChecklistItem(ChecklistStage.MachineChecks, "E-stop released and functional", 0);
        session.AddChecklistItem(ChecklistStage.MachineChecks, "Machine guard in place", 1);
        session.AddChecklistItem(ChecklistStage.MachineChecks, "Coolant levels adequate", 2);

        session.AddChecklistItem(ChecklistStage.Tools, "T01 Face Mill 50mm installed and secured", 0);
        session.AddChecklistItem(ChecklistStage.Tools, "T02 End Mill 10mm (4-flute) installed and secured", 1);

        session.AddChecklistItem(ChecklistStage.Workpiece, "Workpiece material verified", 0);
        session.AddChecklistItem(ChecklistStage.Workpiece, "Torque clamp to 25 Nm", 1);

        return session;
    }

    private static void ConfirmAll(MachineSession session, ChecklistStage stage)
    {
        foreach (var item in session.ChecklistItems.Where(i => i.Stage == stage).ToList())
        {
            session.ConfirmChecklistItem(item.Id);
        }
    }

    // 1. Cannot advance from Machine Checks when checks are incomplete.
    [Fact]
    public void AdvanceStage_WhenMachineChecksIncomplete_ThrowsDomainException()
    {
        var session = CreateSession();

        var ex = Assert.Throws<DomainException>(() => session.AdvanceStage());

        Assert.Contains("must be confirmed", ex.Message);
        Assert.Equal(StageType.MachineChecks, session.CurrentStage);
    }

    // 2. Can advance from Machine Checks when all checks are confirmed.
    [Fact]
    public void AdvanceStage_WhenMachineChecksComplete_MovesToTools()
    {
        var session = CreateSession();
        ConfirmAll(session, ChecklistStage.MachineChecks);

        session.AdvanceStage();

        Assert.Equal(StageType.Tools, session.CurrentStage);
    }

    // 3. Cannot advance from Tools when tools are incomplete.
    [Fact]
    public void AdvanceStage_WhenToolsIncomplete_ThrowsDomainException()
    {
        var session = CreateSession();
        ConfirmAll(session, ChecklistStage.MachineChecks);
        session.AdvanceStage();

        Assert.Equal(StageType.Tools, session.CurrentStage);

        var ex = Assert.Throws<DomainException>(() => session.AdvanceStage());
        Assert.Contains("must be confirmed", ex.Message);
        Assert.Equal(StageType.Tools, session.CurrentStage);
    }

    // 4. Can advance from Tools when all tools are confirmed.
    [Fact]
    public void AdvanceStage_WhenToolsComplete_MovesToWorkpiece()
    {
        var session = CreateSession();
        ConfirmAll(session, ChecklistStage.MachineChecks);
        session.AdvanceStage();
        ConfirmAll(session, ChecklistStage.Tools);

        session.AdvanceStage();

        Assert.Equal(StageType.Workpiece, session.CurrentStage);
    }

    // 5. Cannot advance from Workpiece when setup is incomplete.
    [Fact]
    public void AdvanceStage_WhenWorkpieceIncomplete_ThrowsDomainException()
    {
        var session = CreateSession();
        ConfirmAll(session, ChecklistStage.MachineChecks);
        session.AdvanceStage();
        ConfirmAll(session, ChecklistStage.Tools);
        session.AdvanceStage();

        Assert.Equal(StageType.Workpiece, session.CurrentStage);

        var ex = Assert.Throws<DomainException>(() => session.AdvanceStage());
        Assert.Contains("must be confirmed", ex.Message);
        Assert.Equal(StageType.Workpiece, session.CurrentStage);
    }

    // 6. Can reach Ready Review only after required setup is complete.
    [Fact]
    public void AdvanceStage_WhenWorkpieceComplete_ReachesReadyReview()
    {
        var session = CreateSession();
        ConfirmAll(session, ChecklistStage.MachineChecks);
        session.AdvanceStage();
        ConfirmAll(session, ChecklistStage.Tools);
        session.AdvanceStage();
        ConfirmAll(session, ChecklistStage.Workpiece);

        session.AdvanceStage();

        Assert.Equal(StageType.ReadyReview, session.CurrentStage);
    }

    // 7. Operation cannot start before READY (i.e. not at ReadyReview/Operation).
    [Theory]
    [InlineData(StageType.MachineChecks)]
    [InlineData(StageType.Tools)]
    [InlineData(StageType.Workpiece)]
    public void StartOperation_WhenNotReady_ThrowsDomainException(StageType stage)
    {
        var session = CreateSession();

        if (stage == StageType.Tools)
        {
            ConfirmAll(session, ChecklistStage.MachineChecks);
            session.AdvanceStage();
        }
        else if (stage == StageType.Workpiece)
        {
            ConfirmAll(session, ChecklistStage.MachineChecks);
            session.AdvanceStage();
            ConfirmAll(session, ChecklistStage.Tools);
            session.AdvanceStage();
        }

        Assert.Equal(stage, session.CurrentStage);

        Assert.Throws<DomainException>(() => session.StartOperation());
    }

    // 8. Operation can start when all prerequisites are complete (at ReadyReview).
    [Fact]
    public void StartOperation_WhenAtReadyReview_StartsOperation()
    {
        var session = CreateSession();
        ConfirmAll(session, ChecklistStage.MachineChecks);
        session.AdvanceStage();
        ConfirmAll(session, ChecklistStage.Tools);
        session.AdvanceStage();
        ConfirmAll(session, ChecklistStage.Workpiece);
        session.AdvanceStage();

        Assert.Equal(StageType.ReadyReview, session.CurrentStage);

        session.StartOperation();

        Assert.Equal(StageType.Operation, session.CurrentStage);
        Assert.Equal(OperationStatus.Running, session.OperationStatus);
    }

    // 9. Starting changes READY -> RUNNING.
    [Fact]
    public void StartOperation_TransitionsReadyToRunning()
    {
        var session = CreateSessionWithAllChecked();

        Assert.Equal(OperationStatus.Ready, session.OperationRun!.Status);

        session.StartOperation();

        Assert.Equal(OperationStatus.Running, session.OperationRun!.Status);
        Assert.NotNull(session.OperationRun.StartedAt);
        Assert.Null(session.OperationRun.StoppedAt);
    }

    // 10. Stopping changes RUNNING -> STOPPED.
    [Fact]
    public void StopOperation_TransitionsRunningToStopped()
    {
        var session = CreateSessionWithAllChecked();
        session.StartOperation();

        session.StopOperation();

        Assert.Equal(OperationStatus.Stopped, session.OperationRun!.Status);
        Assert.NotNull(session.OperationRun.StoppedAt);
    }

    // 11. Stopping does not erase the completed setup state.
    [Fact]
    public void StopOperation_PreservesCompletedSetupState()
    {
        var session = CreateSessionWithAllChecked();
        Assert.True(session.IsStageComplete(ChecklistStage.MachineChecks));
        Assert.True(session.IsStageComplete(ChecklistStage.Tools));
        Assert.True(session.IsStageComplete(ChecklistStage.Workpiece));

        session.StartOperation();
        session.StopOperation();

        Assert.Equal(OperationStatus.Stopped, session.OperationRun!.Status);
        Assert.True(session.IsStageComplete(ChecklistStage.MachineChecks));
        Assert.True(session.IsStageComplete(ChecklistStage.Tools));
        Assert.True(session.IsStageComplete(ChecklistStage.Workpiece));
        Assert.Equal(StageType.Operation, session.CurrentStage);
    }

    // 12. Invalid stage transitions are rejected (advance past Operation is not allowed).
    [Fact]
    public void AdvanceStage_PastOperation_ThrowsDomainException()
    {
        var session = CreateSessionWithAllChecked();
        session.StartOperation();

        Assert.Equal(StageType.Operation, session.CurrentStage);
        Assert.Throws<DomainException>(() => session.AdvanceStage());
    }

    // 13. Invalid checklist IDs are handled correctly.
    [Fact]
    public void ConfirmChecklistItem_WithUnknownId_ThrowsDomainException()
    {
        var session = CreateSession();

        var ex = Assert.Throws<DomainException>(() => session.ConfirmChecklistItem(Guid.NewGuid()));
        Assert.Contains("not found", ex.Message);
    }

    // 13. Invalid checklist IDs that are valid but already confirmed are handled.
    [Fact]
    public void ConfirmChecklistItem_WhenAlreadyConfirmed_ThrowsDomainException()
    {
        var session = CreateSession();
        var item = session.ChecklistItems.First(i => i.Stage == ChecklistStage.MachineChecks);
        session.ConfirmChecklistItem(item.Id);

        var ex = Assert.Throws<DomainException>(() => session.ConfirmChecklistItem(item.Id));
        Assert.Contains("already confirmed", ex.Message);
    }

    // DUPLICATE start is rejected (idempotency guard).
    [Fact]
    public void StartOperation_WhenAlreadyRunning_ThrowsDomainException()
    {
        var session = CreateSessionWithAllChecked();
        session.StartOperation();

        Assert.Throws<DomainException>(() => session.StartOperation());
    }

    // Stop before running is rejected.
    [Fact]
    public void StopOperation_WhenNotRunning_ThrowsDomainException()
    {
        var session = CreateSession();

        Assert.Throws<DomainException>(() => session.StopOperation());
    }

    private static MachineSession CreateSessionWithAllChecked()
    {
        var session = CreateSession();
        ConfirmAll(session, ChecklistStage.MachineChecks);
        session.AdvanceStage();
        ConfirmAll(session, ChecklistStage.Tools);
        session.AdvanceStage();
        ConfirmAll(session, ChecklistStage.Workpiece);
        session.AdvanceStage();
        return session; // at ReadyReview
    }
}
