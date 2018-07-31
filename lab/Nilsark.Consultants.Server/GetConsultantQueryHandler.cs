using System.Linq;
using Composable;
using Composable.Messaging.Buses;
using Composable.Persistence.EventStore;
using Nilsark.Consultants.Shared.Domain.Events;
using Nilsark.Consultants.Shared.Queries;
using Nilsark.Consultants.Shared.QueryModels;

namespace Nilsark.Consultants.Server
{
    public class GetConsultantQueryHandler
    {
        private static EventStoreApi EventStoreApi => new ComposableApi().EventStore;

        public static void Register(MessageHandlerRegistrarWithDependencyInjectionSupport registrar) =>
            registrar.ForQuery((GetConsultantQuery query, ILocalApiNavigatorSession bus) =>
                Handle(query, bus));

        private static ConsultantQueryModel Handle(GetConsultantQuery query, ILocalApiNavigatorSession bus)
        {
            var events = bus.Execute(EventStoreApi.Queries.GetHistory<IConsultantEvent>(query.ConsultantId)).ToList();
            return new ConsultantQueryModel(events);
        }
    }
}