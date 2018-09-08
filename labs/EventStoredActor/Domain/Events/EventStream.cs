using System.Runtime.Serialization;
using Common.DDD;
using Domain.Events.Implementation;

namespace Domain.Events
{
    //Used to store the whole event stream in reliable collections
    [DataContract]
    [KnownType(typeof(NameSet))]
    [KnownType(typeof(BarAdded))]
    public class EventStream : EventStreamBase
    { }
}