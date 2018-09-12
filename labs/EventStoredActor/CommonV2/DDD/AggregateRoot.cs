using System;
using Common.DDD;
using Composable.GenericAbstractions.Time;
using Composable.Persistence.EventStore;
using Composable.Persistence.EventStore.Aggregates;

namespace CommonV2.DDD
{
    [AllowPublicSetters] //Temporary, for the compatibility hacks.
    public abstract class AggregateRoot<TAggregate, TAggregateEventImplementation, TAggregateEvent> : Aggregate<TAggregate, TAggregateEventImplementation, TAggregateEvent>, IAggregateRoot
        where TAggregate : Aggregate<TAggregate, TAggregateEventImplementation, TAggregateEvent>
        where TAggregateEvent : class, IAggregateEvent
        where TAggregateEventImplementation : AggregateEvent, TAggregateEvent
    {
        protected AggregateRoot() : base(new DateTimeNowTimeSource())
        {   
        }

        public void RaiseEvent<TEvent>(TEvent @event) where TEvent : TAggregateEventImplementation, IDomainEvent
        {
            Publish(@event);
            EventController.RaiseDomainEvent(@event).GetAwaiter().GetResult();
        }

        [Obsolete("Kept for compatibility with FG.CQRS", false)]
        public IEventController EventController { private get; set; }
    }

    public static class LegacyEventControllerBackwardCompatibilityExtensions
    {
        public static T Get<T>(this IEventStoreUpdater @this, Guid aggregateId, IEventController eventController)
            where T : IEventStored, IAggregateRoot
        {
            var aggregate = @this.Get<T>(aggregateId);
            aggregate.EventController = eventController;
            return aggregate;
        }
    }
}