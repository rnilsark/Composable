using System;
using Composable.Persistence.EventStore;

namespace Common.DDD
{
    public interface IAggregateRootDeletedEvent : IAggregateRootEvent
    {

    }

    public interface IAggregateRootCreatedEvent : IAggregateRootEvent
    {

    }

    public interface IAggregateRootEvent : IDomainEvent, IAggregateEvent
    {
    }
}
