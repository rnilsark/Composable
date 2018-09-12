using System.Collections.Generic;
using System.Runtime.Serialization;
using Composable.Persistence.EventStore.Query.Models.SelfGeneratingQueryModels;
using Domain.Events;

namespace Actor1.Interfaces.ReadModels
{
    //TODO: Composable must support data contract serialization or we need some other way of handling inheritance from SelfGeneratingQueryModel
    public class FooReadModel : SelfGeneratingQueryModel<FooReadModel, IFooEvent>
    {
        public FooReadModel(IEnumerable<IFooEvent> history)
        {
            RegisterEventAppliers()
                .For<IFooNamePropertyUpdated>(e => Name = e.Name);

            LoadFromHistory(history);
        }

        public string Name { get; set; }
    }

    [DataContract]
    public class FooReadModelContract
    {
        [DataMember] public string Name { get; set; }
    }
}