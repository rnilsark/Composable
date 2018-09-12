using System;
using System.Threading;
using System.Threading.Tasks;
using Common.CQRS;
using Composable.DependencyInjection;
using Composable.Messaging.Buses;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace CommonV2.ServiceFabric.Extensions.Actors.Runtime
{
    public abstract class EventStoredActorBaseV2 : Common.ServiceFabric.Extensions.Actors.Runtime.EventStoredActorBase
    {
        private readonly IEndpoint _composableEndpoint;

        protected EventStoredActorBaseV2(ActorService actorService, ActorId actorId, IEndpoint composableEndpoint) : base(actorService, actorId)
        {
            _composableEndpoint = composableEndpoint;
        }

        protected Task<TResult> ExecuteCommandAsync<TResult, TComponent>(Func<TComponent, TResult> func, ICommand command, CancellationToken cancellationToken) where TComponent : class
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
            return Task.FromResult(result);
        }

        protected Task ExecuteCommandAsync<TComponent>(Action<TComponent> action, ICommand command, CancellationToken cancellationToken) where TComponent : class
        {
            var container = _composableEndpoint.ServiceLocator;
            using (container.BeginScope())
            {
                //TODO: Handle command de-duplication with SQL table
                container.ExecuteTransaction(() =>
                {
                    var component = container.Resolve<TComponent>();
                    action(component);
                });
            }
            return Task.FromResult(true);
        }

    }
}