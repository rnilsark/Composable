using System;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Actor1.Interfaces;
using Actor1.Interfaces.ReadModels;
using Composable.Persistence.EventStore;
using Domain.Events;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace Actor1
{
    internal class Actor1V2ActorService : ActorService, IActor1V2ActorService
    {
        private readonly ComposableBootstrapper _composableBootstrapper;

        public Actor1V2ActorService
        (
            StatefulServiceContext context,
            ActorTypeInformation actorTypeInfo,
            ComposableBootstrapper composableBootstrapper,
            Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = null,
            IActorStateProvider stateProvider = null,
            ActorServiceSettings settings = null) :
            base(context, actorTypeInfo, (service, id) => new Actor1V2(service, id, composableBootstrapper.ComposableEndpoint), stateManagerFactory, stateProvider, settings)
        {
            _composableBootstrapper = composableBootstrapper;
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            await base.RunAsync(cancellationToken);
            await _composableBootstrapper.StartAsync();

            if (cancellationToken.CanBeCanceled)
                cancellationToken.ThrowIfCancellationRequested();
        }

        public Task<FooReadModelContract> GetFooAsync(Guid id, CancellationToken cancellationToken)
        {
            var container = _composableBootstrapper.ComposableEndpoint.ServiceLocator;
            using (container.BeginScope())
            {
                var eventStoreReader = container.Resolve<IEventStoreReader>();
                var history = eventStoreReader.GetHistory(id).Cast<IFooEvent>();
                var result = new FooReadModel(history);
                return Task.FromResult(new FooReadModelContract { Name = result.Name });
            }
        }

        public Task<HistoryReadModelContract> GetHistoryAsync(Guid id, CancellationToken cancellationToken)
        {
            var container = _composableBootstrapper.ComposableEndpoint.ServiceLocator;
            using (container.BeginScope())
            {
                var eventStoreReader = container.Resolve<IEventStoreReader>();
                var history = eventStoreReader.GetHistory(id).Cast<IFooEvent>();
                var result = new HistoryReadModel(history);
                return Task.FromResult(new HistoryReadModelContract { EventNames = result.EventNames.ToArray() });
            }
        }
    }
}