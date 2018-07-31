using Composable;
using Composable.Messaging.Buses;
using Composable.Persistence.EventStore;
using Nilsark.Consultants.Server.Domain;
using Nilsark.Consultants.Shared.Commands;

namespace Nilsark.Consultants.Server
{
    public class DismissConsultantCommandHandler
    {
        private static EventStoreApi EventStoreApi => new ComposableApi().EventStore;

        public static void Register(MessageHandlerRegistrarWithDependencyInjectionSupport registrar) =>
            registrar.ForCommand((DismissConsultantCommand cmd, ILocalApiNavigatorSession bus) => Handle(cmd, bus));

        
        private static void Handle(DismissConsultantCommand cmd, ILocalApiNavigatorSession bus)
        {
            bus.Execute(EventStoreApi.Queries.GetForUpdate<Consultant>(cmd.ConsultantId)).Dismiss();
        }
    }
}