using System;
using Composable.DependencyInjection.Persistence;
using Composable.Messaging.Buses;
using Composable.Persistence.EventStore;
using Nilsark.Consultants.Server.Domain;
using Nilsark.Consultants.Shared.Domain.Events;
using Nilsark.Consultants.Shared.QueryModels;

namespace Nilsark.Consultants.Server
{
    public class Bootstrapper
    {
        public IEndpoint RegisterWith(IEndpointHost host)
        {
            return host.RegisterEndpoint("Consultants",
                new EndpointId(Guid.Parse("3183B98B-F487-4FE0-A3FC-33810F74D215")),
                builder =>
                {
                    Shared.TypeMapper.MapTypes(builder.TypeMapper);
                    Server.TypeMapper.MapTypes(builder.TypeMapper);
                    RegisterComponents(builder);
                    RegisterHandlers(builder);
                });
        }

        private static void RegisterComponents(IEndpointBuilder builder)
        {
            builder.Container.RegisterSqlServerEventStore(builder.Configuration.ConnectionStringName)
                .HandleAggregate<Consultant, IConsultantEvent>(builder.RegisterHandlers);

            builder.Container.RegisterSqlServerDocumentDb(builder.Configuration.ConnectionStringName)
                .HandleDocumentType<EventStoreApi.Query.AggregateLink<Consultant>>(builder.RegisterHandlers)
                .HandleDocumentType<ConsultantIdsQueryModel>(builder.RegisterHandlers);
        }

        private static void RegisterHandlers(IEndpointBuilder builder)
        {
            EnrolConsultantCommandHandler.Register(builder.RegisterHandlers);
            ConsultantIdsQueryModelUpdater.Register(builder.RegisterHandlers);
            GetConsultantIdsQueryHandler.Register(builder.RegisterHandlers);
            GetConsultantQueryHandler.Register(builder.RegisterHandlers);
            DismissConsultantAfterCertainTimeSaga.Register(builder.RegisterHandlers);
            DismissConsultantCommandHandler.Register(builder.RegisterHandlers);
        }
    }
}