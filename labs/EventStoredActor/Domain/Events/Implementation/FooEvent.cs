using System;
using System.Runtime.Serialization;
using Common.DDD;

namespace Domain.Events.Implementation
{
    [DataContract]
    public abstract class FooEvent : DomainEventBase
    {
        [DataMember]
        public Guid AggregateRootId { get; set; }
    }

    [DataContract]
    public class NameSet : FooEvent, IFooCreatedEvent
    {
        [DataMember]
        public string Name { get; set; }
    }

    [DataContract]
    public class BarAdded : FooEvent, IBarAdded
    {
        [DataMember]
        public int BarId { get; }
    }
}