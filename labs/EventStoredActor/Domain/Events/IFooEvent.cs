using Common.DDD;
using Composable.Persistence.EventStore;

namespace Domain.Events
{
    public interface IFooEvent : IAggregateRootEvent
    {
    }

    public interface IFooNamePropertyUpdated : IFooEvent
    {
        string Name { get; }
    }
    
    public interface INameSetEvent : IAggregateCreatedEvent, IFooNamePropertyUpdated { }

    public interface IBarEvent : IFooEvent
    {
        int BarId { get; }
    }

    public interface IBarAdded : IBarEvent { }

}