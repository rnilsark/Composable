using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Actor1.Interfaces;
using Actor1.Interfaces.Commands;
using Common.DDD;
using CommonV2.DDD;
using CommonV2.ServiceFabric.Extensions.Actors.Runtime;
using Composable.Messaging.Buses;
using Composable.Persistence.EventStore;
using Domain;
using Domain.Events.Implementation;

namespace Actor1
{
    [StatePersistence(StatePersistence.None)]
    internal class Actor1V2 : 
        EventStoredActorBaseV2, 
        IActor1V2,
        IHandleDomainEvent<CreatedEvent>,
        IHandleDomainEvent<RenamedEvent>,
        IHandleDomainEvent<BarAddedEvent>,
        IRemindable
    {
        public Actor1V2(ActorService actorService, ActorId actorId, IEndpoint composableEndpoint)
            : base(actorService, actorId, composableEndpoint)
        {
        }

        protected override Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");
            return base.OnActivateAsync();
        }

        //Example of when updating an aggregate
        public Task RenameAsync(RenameCommand command, CancellationToken cancellationToken)
        {
            return ExecuteCommandAsync(
                (IEventStoreUpdater eventStoreUpdater) => 
                    eventStoreUpdater.Get<Foo>(this.GetActorId().GetGuidId(), this).Rename(command.Name), command, cancellationToken);
        }

        //Example of when creating an aggregate
        public Task CreateAsync(CreateCommand command, CancellationToken cancellationToken)
        {
            return ExecuteCommandAsync(
                (IEventStoreUpdater eventStoreUpdater) => Foo.Create(this.GetActorId().GetGuidId(), command.Name, eventStoreUpdater, this), 
                command, 
                cancellationToken);
        }

        //Verify backwards compatibility of having "side effects"
        public async Task Handle(CreatedEvent domainEvent)
        {
            await RegisterReminderAsync("note_to_self", null, TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(-1));
            await Log(domainEvent);
        }

        public Task Handle(RenamedEvent domainEvent) => Log(domainEvent);
        public Task Handle(BarAddedEvent domainEvent) => Log(domainEvent);

        public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
        {
            if (reminderName == "note_to_self")
            {
                await RenameAsync(new RenameCommand {Name = "Noted myself"}, CancellationToken.None);
            }
        }

        private static Task Log<T>(T @event) where  T : IDomainEvent
        {
            ActorEventSource.Current.Message($"{typeof(T).FullName}: {@event.UtcTimeStamp:O}");
            return Task.CompletedTask;
        }
    }
}
