using Composable.Persistence.EventStore;

namespace Nilsark.Consultants.Shared.Domain.Events
{
    public interface IConsultantEvent : IAggregateEvent
    {
    }

    public interface IConsultantFullNamePropertyUpdated : IConsultantEvent
    {
        string FullName { get; }
    }

    public interface IConsultantEmailPropertyUpdated : IConsultantEvent
    {
        string Email { get; }
    }

    public interface IConsultantEnrolledEvent : IAggregateCreatedEvent, IConsultantFullNamePropertyUpdated, IConsultantEmailPropertyUpdated
    {
    }

    public interface IConsultantDismissedEvent : IAggregateDeletedEvent
    {
    }
}