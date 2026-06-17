using MediatR;

namespace DentalERP.SharedKernel.Abstractions;

public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}
