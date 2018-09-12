using System;
using System.Threading;
using System.Threading.Tasks;
using Actor1.Interfaces.Commands;
using Actor1.Interfaces.ReadModels;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Remoting.FabricTransport;
using Microsoft.ServiceFabric.Services.Remoting;

[assembly: FabricTransportActorRemotingProvider(RemotingListenerVersion = RemotingListenerVersion.V2_1, RemotingClientVersion = RemotingClientVersion.V2_1)]
namespace Actor1.Interfaces
{
    public interface IActor1V2 : IActor
    {
        Task RenameAsync(RenameCommand command, CancellationToken cancellationToken);
        Task CreateAsync(CreateCommand command, CancellationToken cancellationToken);
    }

    public interface IActor1V2ActorService : IActorService
    {
        Task<FooReadModelContract> GetFooAsync(Guid id, CancellationToken cancellationToken);
        Task<HistoryReadModelContract> GetHistoryAsync(Guid id, CancellationToken cancellationToken);
    }
}
