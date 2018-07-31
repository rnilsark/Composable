using Composable;
using Composable.Functional;
using Composable.Messaging.Buses;
using Composable.Persistence.DocumentDb;
using Nilsark.Consultants.Shared.Queries;
using Nilsark.Consultants.Shared.QueryModels;

namespace Nilsark.Consultants.Server
{
    public class GetConsultantIdsQueryHandler
    {
        private static DocumentDbApi DocumentDbApi => new ComposableApi().DocumentDb;

        public static void Register(MessageHandlerRegistrarWithDependencyInjectionSupport registrar) =>
            registrar.ForQuery((GetConsultantIdsQuery query, ILocalApiNavigatorSession bus) =>
                Handle(query, bus));

        private static ConsultantIdsQueryModel Handle(GetConsultantIdsQuery _, ILocalApiNavigatorSession bus)
        {
            return bus.Execute(DocumentDbApi.Queries.TryGet<ConsultantIdsQueryModel>(ConsultantIdsQueryModelUpdater.ConsultantsIdsQueryModelId)) is Some<ConsultantIdsQueryModel> queryModel ? 
                queryModel.Value : new ConsultantIdsQueryModel(ConsultantIdsQueryModelUpdater.ConsultantsIdsQueryModelId);
        }
    }
}