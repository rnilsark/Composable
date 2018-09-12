using CommonV2.DDD;

namespace Domain.Events
{
    public interface IFooEvent : IAggregateRootEvent
    {
    }

    public interface IFooNamePropertyUpdated : IFooEvent
    {
        string Name { get; }
    }

    public interface ICreatedEvent : IFooNamePropertyUpdated, IAggregateRootCreatedEvent {}
    
    public interface IRenamedEvent : IFooNamePropertyUpdated { }

    public interface IBarEvent : IFooEvent
    {
        int BarId { get; }
    }

    public interface IBarAdded : IBarEvent { }

}