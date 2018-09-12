using Composable.Persistence.EventStore;

namespace CommonV2.DDD
{
    public interface IAggregateRootDeletedEvent : IAggregateRootEvent, IAggregateDeletedEvent
    {

    }

    public interface IAggregateRootCreatedEvent : IAggregateRootEvent, IAggregateCreatedEvent
    {

    }

    public interface IAggregateRootEvent : IAggregateEvent
    {
    }
}
