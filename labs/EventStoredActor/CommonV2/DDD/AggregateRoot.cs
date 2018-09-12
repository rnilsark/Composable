using Composable.GenericAbstractions.Time;
using Composable.Persistence.EventStore;
using Composable.Persistence.EventStore.Aggregates;

namespace CommonV2.DDD
{
    public partial class AggregateRoot<TAggregate, TAggregateEventImplementation, TAggregateEvent> : Aggregate<TAggregate, TAggregateEventImplementation, TAggregateEvent>
        where TAggregate : Aggregate<TAggregate, TAggregateEventImplementation, TAggregateEvent>
        where TAggregateEvent : class, IAggregateEvent
        where TAggregateEventImplementation : AggregateEvent, TAggregateEvent
    {
        public AggregateRoot() : base(new DateTimeNowTimeSource())
        {   
        }
    }
}