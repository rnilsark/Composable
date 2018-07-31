using System.Collections.Generic;
using Composable.Persistence.EventStore.Query.Models.SelfGeneratingQueryModels;
using Nilsark.Consultants.Shared.Domain.Events;

namespace Nilsark.Consultants.Shared.QueryModels
{
    public class ConsultantQueryModel : SelfGeneratingQueryModel<ConsultantQueryModel, IConsultantEvent>
    {
        private ConsultantQueryModel()
        {
        }

        public ConsultantQueryModel(IEnumerable<IConsultantEvent> eventHistory)
        {
            RegisterEventAppliers()
                .For<IConsultantFullNamePropertyUpdated>(e => FullName = e.FullName)
                .For<IConsultantEmailPropertyUpdated>(e => Email = e.Email)
                .IgnoreUnhandled<IConsultantEvent>();

            LoadFromHistory(eventHistory);
        }

        public string Email { get; private set; }
        
        public string FullName { get; private set; }
    }
}