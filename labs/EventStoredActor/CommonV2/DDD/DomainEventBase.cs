using System;
using Common.DDD;
using Composable.Persistence.EventStore;

namespace CommonV2.DDD
{
    public abstract class DomainEventBase : AggregateEvent, IAggregateRootEvent, IDomainEvent
    {
        protected DomainEventBase(Guid aggregateId) : base(aggregateId)
        {

        }

        protected DomainEventBase() : base()
        {
        }
    }
}