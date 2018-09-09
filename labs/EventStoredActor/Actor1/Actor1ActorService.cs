using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Actor1.Interfaces;
using Composable.Messaging.Buses;
using Composable.Persistence.EventStore;
using Domain.Events;
using Microsoft.ServiceFabric.Actors.Query;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace Actor1
{
    internal class Actor1ActorService : ActorService, IActor1ActorService
    {
        private readonly IEndpoint _composableEndpoint;
        //private readonly ActorStateProviderEventStreamReader<EventStream> _stateProviderEventStreamReader;

        public Actor1ActorService
        (
            StatefulServiceContext context,
            ActorTypeInformation actorTypeInfo,
            IEndpoint composableEndpoint,
            Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = null,
            IActorStateProvider stateProvider = null, // Handles changes to state
            ActorServiceSettings settings = null) :
            base(context, actorTypeInfo, (service, id) => new Actor1(service, id, composableEndpoint), stateManagerFactory, stateProvider, settings)
        {
            _composableEndpoint = composableEndpoint;
            //_stateProviderEventStreamReader = new ActorStateProviderEventStreamReader<EventStream>(StateProvider, Actor1.EventStreamStateKey);
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            await base.RunAsync(cancellationToken);

            if (cancellationToken.CanBeCanceled)
                cancellationToken.ThrowIfCancellationRequested();
            
        }

        #region Not important

        public async Task<FooReadModelContract[]> GetAllAsync(CancellationToken cancellationToken)
        {
            var aggregateRootIds = await GetAllGuidsAsync(int.MaxValue, cancellationToken);
            var getTasks = aggregateRootIds.Select(id => GetAsync(id, cancellationToken));

            return await Task.WhenAll(getTasks);
        }

        //To test performance of paging.
        public async Task<IEnumerable<Guid>> GetAllGuidsAsync(int numItemsToReturn, CancellationToken cancellationToken)
        {
            ContinuationToken continuationToken = null;

            var ids = new List<Guid>();
            do
            {
                var page = await StateProvider.GetActorsAsync(numItemsToReturn, continuationToken, cancellationToken);
                ids.AddRange(page.Items.Select(actorId => actorId.GetGuidId()));

                continuationToken = page.ContinuationToken;
            } while (continuationToken != null);

            return ids;
        }

        #endregion

        public Task<FooReadModelContract> GetAsync(Guid id, CancellationToken cancellationToken)
        {
            var container = _composableEndpoint.ServiceLocator;
            using (container.BeginScope())
            {
                var eventStoreReader = container.Resolve<IEventStoreReader>();
                var history = eventStoreReader.GetHistory(id).Cast<IFooEvent>();
                var result = new FooReadModel(history);
                return Task.FromResult(new FooReadModelContract { Name = result.Name });
            }
        }
    }
}