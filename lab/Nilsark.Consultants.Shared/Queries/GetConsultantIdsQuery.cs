using Composable.Messaging;
using Nilsark.Consultants.Shared.QueryModels;

namespace Nilsark.Consultants.Shared.Queries
{
    public class GetConsultantIdsQuery : BusApi.Remotable.NonTransactional.IQuery<ConsultantIdsQueryModel>
    {
        public GetConsultantIdsQuery()
        {
        }
    }
}