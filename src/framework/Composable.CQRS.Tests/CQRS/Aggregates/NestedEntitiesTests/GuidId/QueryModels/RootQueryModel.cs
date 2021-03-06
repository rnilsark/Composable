using System;
using Composable.Persistence.EventStore.Query.Models.SelfGeneratingQueryModels;
using Composable.Tests.CQRS.Aggregates.NestedEntitiesTests.GuidId.Domain.Events;

namespace Composable.Tests.CQRS.Aggregates.NestedEntitiesTests.GuidId.QueryModels
{
    class RootQueryModel : SelfGeneratingQueryModel<RootQueryModel, RootEvent.IRoot>
    {
        public string Name { get; private set; }
        readonly Entity.CollectionManager _entities;
#pragma warning disable 108,114
        public Component Component { get; private set; }
#pragma warning restore 108,114

        public RootQueryModel()
        {
            Component = new Component(this);
            _entities = Entity.CreateSelfManagingCollection(this);

            RegisterEventAppliers()
                .For<RootEvent.PropertyUpdated.Name>(e => Name = e.Name);
        }

        public IReadonlyQueryModelEntityCollection<Entity, Guid> Entities => _entities.Entities;
    }
}
