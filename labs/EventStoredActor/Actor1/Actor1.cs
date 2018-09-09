using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Actor1.Interfaces;
using Actor1.Interfaces.Commands;
using Common.DDD;
using Common.ServiceFabric.Extensions.Actors.Runtime;
using Composable.DependencyInjection;
using Composable.Messaging.Buses;
using Composable.Persistence.EventStore;
using Domain;
using Domain.Events;
using Domain.Events.Implementation;

namespace Actor1
{
    /// <remarks>
    /// This class represents an actor.
    /// Every ActorID maps to an instance of this class.
    /// The StatePersistence attribute determines persistence and replication of actor state:
    ///  - Persisted: State is written to disk and replicated.
    ///  - Volatile: State is kept in memory only and replicated.
    ///  - None: State is kept in memory only and not replicated.
    /// </remarks>
    [StatePersistence(StatePersistence.Persisted)]
    internal class Actor1 : 
        EventStoredActorBase, 
        IActor1 
        //IHandleDomainEvent<NameSet>, 
        //IHandleDomainEvent<BarAdded>
    {
        private readonly IEndpoint _composableEndpoint;

        /// <summary>
        /// Initializes a new instance of Actor1
        /// </summary>
        /// <param name="actorService">The Microsoft.ServiceFabric.Actors.Runtime.ActorService that will host this actor instance.</param>
        /// <param name="actorId">The Microsoft.ServiceFabric.Actors.ActorId for this actor instance.</param>
        /// <param name="composableEndpoint"></param>
        public Actor1(ActorService actorService, ActorId actorId, IEndpoint composableEndpoint)
            : base(actorService, actorId)
        {
            _composableEndpoint = composableEndpoint;
        }

        /// <summary>
        /// This method is called whenever an actor is activated.
        /// An actor is activated the first time any of its methods are invoked.
        /// </summary>
        protected override async Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");
            //await GetAndSetDomainAsync(); // Loads event stream from disk.
        }

        //public Task EditNameAsync(SetNameCommand command)
        //{
        //    var container = _composableEndpoint.ServiceLocator;
        //    using (container.BeginScope())
        //    {
        //        container.ExecuteTransaction(() =>
        //        {
        //            var eventStoreUpdater = container.Resolve<IEventStoreUpdater>();
        //            var foo = eventStoreUpdater.Get<Foo>(this.GetActorId().GetGuidId());
        //            foo.SetName(this.GetActorId().GetGuidId(), command.Name);                    
        //        });
        //    }
        //}

        private TResult RunTransaction<TResult, TComponent>(Func<TComponent, TResult> func) where TComponent : class
        {
            TResult result = default(TResult);
            var container = _composableEndpoint.ServiceLocator;
            using (container.BeginScope())
            {
                container.ExecuteTransaction(() =>
                {
                    var component = container.Resolve<TComponent>();
                    result = func(component);
                });
            }

            return result;

        } 

        public Task SetNameAsync(SetNameCommand command, CancellationToken cancellationToken)
        {
            return RunTransaction((IEventStoreUpdater eventStoreUpdater) =>
            {
                var foo = new Foo();
                foo.SetName(this.GetActorId().GetGuidId(), command.Name);                    
                eventStoreUpdater.Save(foo);
                return Task.CompletedTask;
            });

            //var container = _composableEndpoint.ServiceLocator;
            //using (container.BeginScope())
            //{
            //    container.ExecuteTransaction(() =>
            //    {
            //        var eventStoreUpdater = container.Resolve<IEventStoreUpdater>();

            //        var foo = new Foo();
            //        foo.SetName(this.GetActorId().GetGuidId(), command.Name);                    
            //        eventStoreUpdater.Save(foo);
            //    });
            //}


            //// Handles deduplication
            ////return ExecuteCommandAsync(
            ////    () => DomainState.SetName(this.GetActorId().GetGuidId(), command.Name),
            ////    command,
            ////    cancellationToken);

            ////await UpdateQueryModel();
            //return Task.CompletedTask;
        }

        // Sets state (commited when actor method finnishes)
        //public async Task Handle(NameSet domainEvent)
        //{
            //await StoreDomainEventAsync(domainEvent);
            //await UpdateQueryModel();
        //}

        //public async Task Handle(BarAdded domainEvent) => await StoreDomainEventAsync(domainEvent);
    }
}
