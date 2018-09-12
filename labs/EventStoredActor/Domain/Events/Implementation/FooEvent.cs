using System;
using CommonV2.DDD;
using Composable.Persistence.EventStore.Aggregates;

namespace Domain.Events.Implementation
{
    public abstract class FooEvent : DomainEventBase, IFooEvent
    {
        protected FooEvent()
        {
        }

        protected FooEvent(Guid aggregateId) : base(aggregateId)
        {
        }
    }

    public class CreatedEvent : FooEvent, ICreatedEvent
    {
        public CreatedEvent(Guid id, string name) : base(id)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public class RenamedEvent : FooEvent, IRenamedEvent 
    {
        public RenamedEvent(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public class BarEvent : FooEvent, IBarEvent
    {
        public int BarId { get; protected set; }

        public class IdGetterSetter : BarEvent, IGetSetAggregateEntityEventEntityId<int, BarEvent, IBarEvent> //This is different from FG.CQRS, but cleaner
        {
            public void SetEntityId(BarEvent @event, int id) => @event.BarId = id;
            public int GetId(IBarEvent @event) => @event.BarId;
        }
    }

    public class BarAddedEvent : BarEvent, IBarAdded
    {
       
    }
}