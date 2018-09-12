using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Actor1.Interfaces;
using Actor1.Interfaces.Commands;
using Common.ServiceFabric.Extensions.Actors.Runtime;
using Composable.DependencyInjection;
using Composable.Messaging.Buses;
using Composable.Persistence.EventStore;
using Domain;

namespace Actor1
{
    [StatePersistence(StatePersistence.None)]
    internal class Actor1V2 : 
        EventStoredActorBase, 
        IActor1V2
    {
        private readonly IEndpoint _composableEndpoint;
        
        public Actor1V2(ActorService actorService, ActorId actorId, IEndpoint composableEndpoint)
            : base(actorService, actorId)
        {
            _composableEndpoint = composableEndpoint;
        }

        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");
            return base.OnActivateAsync();
        }

        public Task RenameAsync(RenameCommand command, CancellationToken cancellationToken)
        {
            return RunTransaction((IEventStoreUpdater eventStoreUpdater) =>
            {
                eventStoreUpdater
                    .Get<Foo>(this.GetActorId().GetGuidId())
                    .Rename(command.Name);
                
                return Task.FromResult(true);
            });
        }

        public Task CreateAsync(CreateCommand command, CancellationToken cancellationToken)
        {
            return RunTransaction((IEventStoreUpdater eventStoreUpdater) =>
            {
                Foo.Create(this.GetActorId().GetGuidId(), command.Name, eventStoreUpdater);
                return Task.FromResult(true);
            });
        }
        
        private TResult RunTransaction<TResult, TComponent>(Func<TComponent, TResult> func) where TComponent : class
        {
            var result = default(TResult);
            
            var container = _composableEndpoint.ServiceLocator;
            using (container.BeginScope())
            {
                //TODO: Handle command de-duplication with SQL table
                container.ExecuteTransaction(() =>
                {
                    var component = container.Resolve<TComponent>();
                    result = func(component);
                });
            }

            return result;
        }

    }
}
