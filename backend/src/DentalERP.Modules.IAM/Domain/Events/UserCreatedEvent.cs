using DentalERP.SharedKernel.Abstractions;

namespace DentalERP.Modules.IAM.Domain.Events;

public sealed record UserCreatedEvent(Guid UserId, string Username) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
