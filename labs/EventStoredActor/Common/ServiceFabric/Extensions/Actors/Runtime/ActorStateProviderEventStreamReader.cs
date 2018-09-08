using System;
using System.Threading;
using System.Threading.Tasks;
using Common.DDD;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;

namespace Actor1
{
    public class ActorStateProviderEventStreamReader<TEventStream> : IEventStreamReader where TEventStream : EventStreamBase
    {
        private readonly IActorStateProvider _stateProvider;
        private readonly string _stateKey;

        public ActorStateProviderEventStreamReader(IActorStateProvider stateProvider, string stateKey)
        {
            _stateProvider = stateProvider;
            _stateKey = stateKey;
        }

        public async Task<IDomainEvent[]> GetEventStream(Guid id, CancellationToken cancellationToken)
        {
            var eventStream = await _stateProvider.LoadStateAsync<TEventStream>(
                new ActorId(id),
                _stateKey,
                cancellationToken);

            return eventStream.DomainEvents;
        }
    }
}