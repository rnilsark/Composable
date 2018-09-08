using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Actor1.Interfaces.Commands;
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
        Task<FooReadModel[]> GetAllAsync(CancellationToken cancellationToken);
        Task<FooReadModel> GetAsync(Guid id, CancellationToken cancellationToken);
    }

    [DataContract]
    public class FooReadModel
    {
        [DataMember]
        public string Name { get; set; }
    }
}
