using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Actor1.Interfaces;
using Actor1.ReadModels;
using Domain.Events;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Query;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace Actor1
{
    internal class Actor1ActorService : ActorService, IActor1ActorService
    {
        private readonly ActorStateProviderEventStreamReader<EventStream> _stateProviderEventStreamReader;

        public Actor1ActorService
        (
            StatefulServiceContext context,
            ActorTypeInformation actorTypeInfo,
            Func<ActorService, ActorId, ActorBase> actorFactory = null,
            Func<ActorBase, IActorStateProvider, IActorStateManager> stateManagerFactory = null,
            IActorStateProvider stateProvider = null, // Handles changes to state
            ActorServiceSettings settings = null) :
            base(context, actorTypeInfo, actorFactory, stateManagerFactory, stateProvider, settings)
        {
            _stateProviderEventStreamReader = new ActorStateProviderEventStreamReader<EventStream>(StateProvider, Actor1.EventStreamStateKey);
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            await base.RunAsync(cancellationToken);

            if (cancellationToken.CanBeCanceled)
                cancellationToken.ThrowIfCancellationRequested();

        }

        public async Task<FooReadModel[]> GetAllAsync(CancellationToken cancellationToken)
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

        public Task<FooReadModel> GetAsync(Guid id, CancellationToken cancellationToken)
        {
            Task<FooReadModel> model = null;
            using (var generator = new ReadModelGenerator(_stateProviderEventStreamReader))
            {
                model = generator.TryGenerateAsync(id, cancellationToken);
            }
            return model;
        }
    }
}