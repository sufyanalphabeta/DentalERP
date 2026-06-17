using DentalERP.Modules.Patients.Domain.Entities;
using Xunit;

namespace DentalERP.UnitTests.Modules.Patients.Domain;

public class QueueEntryTests
{
    [Fact]
    public void Create_SetsTokenAndStatusWaiting()
    {
        var entry = QueueEntry.Create(Guid.NewGuid(), 5);
        Assert.Equal(5, entry.TokenNumber);
        Assert.Equal(QueueStatus.Waiting, entry.Status);
    }

    [Fact]
    public void Call_SetsCalledAt()
    {
        var entry = QueueEntry.Create(Guid.NewGuid(), 1);
        entry.Call();
        Assert.Equal(QueueStatus.Called, entry.Status);
        Assert.NotNull(entry.CalledAt);
    }

    [Fact]
    public void Complete_SetsCompletedAt()
    {
        var entry = QueueEntry.Create(Guid.NewGuid(), 2);
        entry.Call();
        entry.Start();
        entry.Complete();
        Assert.Equal(QueueStatus.Completed, entry.Status);
        Assert.NotNull(entry.CompletedAt);
    }

    [Fact]
    public void Skip_SetsStatusSkipped()
    {
        var entry = QueueEntry.Create(Guid.NewGuid(), 3);
        entry.Skip();
        Assert.Equal(QueueStatus.Skipped, entry.Status);
    }

    [Fact]
    public void ResetToWaiting_ClearsCalledAt()
    {
        var entry = QueueEntry.Create(Guid.NewGuid(), 4);
        entry.Call();
        entry.ResetToWaiting();
        Assert.Equal(QueueStatus.Waiting, entry.Status);
        Assert.Null(entry.CalledAt);
    }
}
