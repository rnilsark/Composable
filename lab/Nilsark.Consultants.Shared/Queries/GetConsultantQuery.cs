using System;
using Composable.Messaging;
using Nilsark.Consultants.Shared.QueryModels;

namespace Nilsark.Consultants.Shared.Queries
{
    public class GetConsultantQuery : BusApi.Remotable.NonTransactional.IQuery<ConsultantQueryModel>
    {
        public GetConsultantQuery(Guid consultantId)
        {
            ConsultantId = consultantId;
        }

        public Guid ConsultantId { get; }
    }
}