using VMWorkflow.Application.Services;
using VMWorkflow.Domain.Enums;
using Xunit;

namespace VMWorkflow.Tests;

public class WorkflowEngineTests
{
    private readonly WorkflowEngine _engine = new();

    [Theory]
    [InlineData(RequestStatus.Draft, RequestStatus.PendingSysAdmin, true)]
    [InlineData(RequestStatus.PendingSysAdmin, RequestStatus.DataCenterReview, true)]
    [InlineData(RequestStatus.DataCenterReview, RequestStatus.PendingNOC, true)]
    [InlineData(RequestStatus.PendingIOCApproval, RequestStatus.PendingApproval, true)]
    [InlineData(RequestStatus.PendingApproval, RequestStatus.Approved, true)]
    [InlineData(RequestStatus.Approved, RequestStatus.Implemented, true)]
    [InlineData(RequestStatus.Implemented, RequestStatus.Closed, true)]
    [InlineData(RequestStatus.Draft, RequestStatus.Approved, false)]
    [InlineData(RequestStatus.PendingSysAdmin, RequestStatus.Approved, false)]
    [InlineData(RequestStatus.Closed, RequestStatus.Draft, false)]
    [InlineData(RequestStatus.PendingNOC, RequestStatus.Closed, false)]
    public void CanTransition_ReturnsExpectedResult(RequestStatus from, RequestStatus to, bool expected)
    {
        var result = _engine.CanTransition(from, to);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void IOCReady_OnlyWhenBothNOCAndSOCComplete()
    {
        Assert.True(_engine.IsIOCReady(nocCompleted: true, socCompleted: true));
        Assert.False(_engine.IsIOCReady(nocCompleted: true, socCompleted: false));
        Assert.False(_engine.IsIOCReady(nocCompleted: false, socCompleted: true));
        Assert.False(_engine.IsIOCReady(nocCompleted: false, socCompleted: false));
    }

    [Fact]
    public void GetNextStatusAfterSysAdmin_FromPendingSysAdmin_ReturnsDataCenterReview()
    {
        var result = _engine.GetNextStatusAfterSysAdmin(RequestStatus.PendingSysAdmin);
        Assert.Equal(RequestStatus.DataCenterReview, result);
    }

    [Fact]
    public void GetNextStatusAfterSysAdmin_FromWrongStatus_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _engine.GetNextStatusAfterSysAdmin(RequestStatus.Draft));
    }

    [Fact]
    public void GetNextStatusAfterDataCenter_FromDataCenterReview_ReturnsPendingNOC()
    {
        var result = _engine.GetNextStatusAfterDataCenter(RequestStatus.DataCenterReview);
        Assert.Equal(RequestStatus.PendingNOC, result);
    }

    [Fact]
    public void GetNextStatusAfterDataCenter_FromWrongStatus_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _engine.GetNextStatusAfterDataCenter(RequestStatus.Draft));
    }

    [Theory]
    [InlineData(RequestStatus.PendingNOC)]
    [InlineData(RequestStatus.PendingSOC)]
    public void GetNextStatusAfterNOC_FromValidStatus_ReturnsCurrentStatus(RequestStatus status)
    {
        var result = _engine.GetNextStatusAfterNOC(status);
        Assert.Equal(status, result);
    }

    [Fact]
    public void GetNextStatusAfterNOC_FromWrongStatus_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _engine.GetNextStatusAfterNOC(RequestStatus.Draft));
    }

    [Fact]
    public void Rejection_AllowsReturnToDraft()
    {
        Assert.True(_engine.CanTransition(RequestStatus.PendingIOCApproval, RequestStatus.Draft));
        Assert.True(_engine.CanTransition(RequestStatus.PendingApproval, RequestStatus.Draft));
        Assert.True(_engine.CanTransition(RequestStatus.Rejected, RequestStatus.Draft));
    }

    [Fact]
    public void ApprovalTransitions_AreValid()
    {
        Assert.True(_engine.CanTransition(RequestStatus.PendingApproval, RequestStatus.Approved));
        Assert.True(_engine.CanTransition(RequestStatus.PendingApproval, RequestStatus.Rejected));
        Assert.True(_engine.CanTransition(RequestStatus.PendingApproval, RequestStatus.Draft));
    }

    [Theory]
    [InlineData(RequestStatus.PendingSysAdmin, RequestStatus.Draft)]
    [InlineData(RequestStatus.DataCenterReview, RequestStatus.PendingSysAdmin)]
    [InlineData(RequestStatus.PendingNOC, RequestStatus.DataCenterReview)]
    [InlineData(RequestStatus.PendingSOC, RequestStatus.DataCenterReview)]
    [InlineData(RequestStatus.PendingIOCApproval, RequestStatus.PendingNOC)]
    [InlineData(RequestStatus.PendingApproval, RequestStatus.PendingIOCApproval)]
    public void GetPreviousStatus_ReturnsCorrectPreviousStep(RequestStatus current, RequestStatus expectedPrevious)
    {
        var result = _engine.GetPreviousStatus(current);
        Assert.Equal(expectedPrevious, result);
    }

    [Fact]
    public void GetPreviousStatus_FromDraft_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _engine.GetPreviousStatus(RequestStatus.Draft));
    }

    [Fact]
    public void GetPreviousStatus_FromApproved_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            _engine.GetPreviousStatus(RequestStatus.Approved));
    }

    [Theory]
    [InlineData(RequestStatus.PendingSysAdmin, RequestStatus.Draft)]
    [InlineData(RequestStatus.DataCenterReview, RequestStatus.PendingSysAdmin)]
    [InlineData(RequestStatus.PendingNOC, RequestStatus.DataCenterReview)]
    [InlineData(RequestStatus.PendingSOC, RequestStatus.DataCenterReview)]
    [InlineData(RequestStatus.PendingIOCApproval, RequestStatus.PendingNOC)]
    [InlineData(RequestStatus.PendingApproval, RequestStatus.PendingIOCApproval)]
    public void SendBack_TransitionsAreAllowed(RequestStatus from, RequestStatus to)
    {
        Assert.True(_engine.CanTransition(from, to));
    }

    // ===== Approval Tests (CISO + Ops Manager) =====

    [Fact]
    public void HasFullApproval_BothApproved_ReturnsTrue()
    {
        Assert.True(_engine.HasFullApproval("Approved", "Approved"));
    }

    [Fact]
    public void HasFullApproval_NotBoth_ReturnsFalse()
    {
        Assert.False(_engine.HasFullApproval("Approved", null));
        Assert.False(_engine.HasFullApproval(null, "Approved"));
        Assert.False(_engine.HasFullApproval(null, null));
    }

    [Fact]
    public void HasRejection_AnyRejected_ReturnsTrue()
    {
        Assert.True(_engine.HasRejection("Rejected", null));
        Assert.True(_engine.HasRejection(null, "Rejected"));
        Assert.True(_engine.HasRejection("Rejected", "Rejected"));
    }

    [Fact]
    public void HasRejection_NoneRejected_ReturnsFalse()
    {
        Assert.False(_engine.HasRejection("Approved", "Approved"));
        Assert.False(_engine.HasRejection("Approved", null));
        Assert.False(_engine.HasRejection(null, null));
    }
}
