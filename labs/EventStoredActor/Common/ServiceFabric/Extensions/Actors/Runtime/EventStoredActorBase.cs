using System;
using System.Threading.Tasks;
using Common.DDD;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace Common.ServiceFabric.Extensions.Actors.Runtime
{
    public abstract class EventStoredActorBase : Actor, IEventController
    {
        protected EventStoredActorBase(ActorService actorService, ActorId actorId) : base(actorService, actorId)
        {
        }

        public async Task RaiseDomainEvent<TDomainEvent>(TDomainEvent domainEvent) where TDomainEvent : IDomainEvent
        {
            if (!(this is IHandleDomainEvent<TDomainEvent> handleDomainEvent))
            {
                throw new Exception($"No handler found for {domainEvent}.");
            }

            await handleDomainEvent.Handle(domainEvent);
        }
    }
}