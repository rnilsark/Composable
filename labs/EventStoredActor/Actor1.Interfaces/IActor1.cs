using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Actor1.Interfaces.Commands;
using Composable.Persistence.EventStore.Query.Models.SelfGeneratingQueryModels;
using Domain.Events;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting;

[assembly: FabricTransportActorRemotingProvider(RemotingListenerVersion = RemotingListenerVersion.V2_1, RemotingClientVersion = RemotingClientVersion.V2_1)]
namespace Actor1.Interfaces
{
    public interface IActor1 : IActor
    {
        Task SetNameAsync(SetNameCommand command, CancellationToken cancellationToken);
    }

    public interface IActor1ActorService : IActorService, IService
    {
        Task<FooReadModelContract[]> GetAllAsync(CancellationToken cancellationToken);
        Task<FooReadModelContract> GetAsync(Guid id, CancellationToken cancellationToken);
    }

    [DataContract]
    public class FooReadModelContract
    {
        [DataMember] public string Name { get; set; }
    }

    public class FooReadModel : SelfGeneratingQueryModel<FooReadModel, IFooEvent>
    {
        public FooReadModel(IEnumerable<IFooEvent> history)
        {
            RegisterEventAppliers()
                .For<IFooNamePropertyUpdated>(e => Name = e.Name);

            LoadFromHistory(history);
        }

        public string Name { get; set; }
    }
}
