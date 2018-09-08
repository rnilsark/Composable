using Common.DDD;

namespace Domain.Events
{
    public interface IFooEvent : IAggregateRootEvent
    {
    }

    public interface IFooNamePropertyUpdated : IFooEvent
    {
        string Name { get; set; }
    }
    
    public interface IFooCreatedEvent : IAggregateRootCreatedEvent, IFooNamePropertyUpdated { }

    public interface IBarEvent : IFooEvent
    {
        int BarId { get; }
    }

    public interface IBarAdded : IBarEvent { }

}