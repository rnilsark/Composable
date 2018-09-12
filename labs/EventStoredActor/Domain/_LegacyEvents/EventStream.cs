using System.Runtime.Serialization;
using Common.DDD;
using Domain.Events.Implementation;

namespace Domain.Events
{
    //Used to store the whole event stream in reliable collections
    [DataContract]
    [KnownType(typeof(CreatedEvent))]
    [KnownType(typeof(RenamedEvent))]
    [KnownType(typeof(BarAddedEvent))]
    public class EventStream : EventStreamBase
    { }
}