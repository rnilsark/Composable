using System;
using System.Runtime.Serialization;
using Common.DDD;
using Composable.Persistence.EventStore;
using Composable.Persistence.EventStore.Aggregates;

namespace Domain.Events.Implementation
{
    public abstract class FooEvent : AggregateEvent, IAggregateRootEvent, IFooEvent
    {
        protected FooEvent()
        {
            
        }

        protected FooEvent(Guid aggregateId) : base(aggregateId)
        {
        }
    }

    public class NameSet : FooEvent, INameSetEvent 
    {
        public NameSet(Guid id, string name) : base(id)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public class BarEvent : FooEvent, IBarEvent
    {
        public int BarId { get; protected set; }

        public class IdGetterSetter : BarEvent, IGetSetAggregateEntityEventEntityId<int, BarEvent, IBarEvent>
        {
            public void SetEntityId(BarEvent @event, int id) => @event.BarId = id;
            public int GetId(IBarEvent @event) => @event.BarId;
        }
    }

    public class BarAdded : BarEvent, IBarAdded
    {
       
    }
}