using System;
using Composable.Persistence.EventStore;

namespace CommonV2.DDD
{
    public abstract class DomainEventBase : AggregateEvent, IAggregateRootEvent
    {
        protected DomainEventBase(Guid aggregateId) : base(aggregateId)
        {

        }

        protected DomainEventBase() : base()
        {
        }
    }
}