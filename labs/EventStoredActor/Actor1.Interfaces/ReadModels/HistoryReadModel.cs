using System.Collections.Generic;
using System.Runtime.Serialization;
using Composable.Persistence.EventStore.Query.Models.SelfGeneratingQueryModels;
using Domain.Events;

namespace Actor1.Interfaces.ReadModels
{
    //TODO: Composable must support data contract serialization or we need some other way of handling inheritance from SelfGeneratingQueryModel
    public class HistoryReadModel : SelfGeneratingQueryModel<HistoryReadModel, IFooEvent>
    {
        public HistoryReadModel(IEnumerable<IFooEvent> history)
        {
            RegisterEventAppliers()
                .For<IFooEvent>(e =>
                {
                    EventNames.Add(e.GetType().Name);
                });

            LoadFromHistory(history);
        }

        public List<string> EventNames { get; } = new List<string>();
    }

    [DataContract]
    public class HistoryReadModelContract
    {
        [DataMember] public string[] EventNames { get; set; }
    }
}