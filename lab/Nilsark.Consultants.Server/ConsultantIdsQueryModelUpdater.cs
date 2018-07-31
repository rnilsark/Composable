using System;
using Composable;
using Composable.Functional;
using Composable.Messaging.Buses;
using Composable.Persistence.DocumentDb;
using Composable.Persistence.EventStore;
using Nilsark.Consultants.Shared.Domain.Events;
using Nilsark.Consultants.Shared.QueryModels;

namespace Nilsark.Consultants.Server
{
    public class ConsultantIdsQueryModelUpdater
    {
        private static DocumentDbApi DocumentDbApi => new ComposableApi().DocumentDb;
        
        public static Guid ConsultantsIdsQueryModelId = Guid.Parse("4C46A560-A9BB-4613-8B0F-0D0A7CDD2716");

        public static void Register(MessageHandlerRegistrarWithDependencyInjectionSupport registrar) =>
            registrar
                .ForEvent((IConsultantEnrolledEvent e, ILocalApiNavigatorSession bus) => Handle(e, bus))
                .ForEvent((IConsultantDismissedEvent e, ILocalApiNavigatorSession bus) => Handle(e, bus));

        private static void Handle(IAggregateCreatedEvent e, ILocalApiNavigatorSession bus)
        {
            if (bus.Execute(DocumentDbApi.Queries.TryGet<ConsultantIdsQueryModel>(ConsultantsIdsQueryModelId)) is
                Some<ConsultantIdsQueryModel> existingQueryModel)
            {
                existingQueryModel.Value.ConsultantIds.Add(e.AggregateId);
            }
            else
            {
                bus.Execute(DocumentDbApi.Commands.Save(ConsultantsIdsQueryModelId.ToString(),
                    new ConsultantIdsQueryModel(ConsultantsIdsQueryModelId)
                    {
                        ConsultantIds = { e.AggregateId }
                    }));
            }
        }

        private static void Handle(IAggregateDeletedEvent e, ILocalApiNavigatorSession bus)
        {
            bus
                .Execute(DocumentDbApi.Queries.GetForUpdate<ConsultantIdsQueryModel>(ConsultantsIdsQueryModelId))
                .ConsultantIds.Remove(e.AggregateId);
        }
    }
}