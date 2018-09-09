using System;
using Composable.Persistence.EventStore;

namespace Common.DDD
{
    public interface IDomainEvent
    {
        Guid EventId { get; }
        DateTime UtcTimeStamp { get; }
    }
}