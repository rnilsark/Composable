using System;
using System.Collections.Generic;
using System.Linq;
using Common.DDD;
using Composable.GenericAbstractions.Time;
using Composable.Persistence.EventStore.Aggregates;
using Domain.Events;
using Domain.Events.Implementation;

namespace Domain
{
    public partial class Foo : Aggregate<Foo, FooEvent, IFooEvent>
    {
        public Foo() : base(new DateTimeNowTimeSource())
        {
            RegisterEventAppliers()
                .For<INameSetEvent>(Apply)
                .For<IFooNamePropertyUpdated>(e => Name = e.Name);

            _bars = Bar.CreateSelfManagingCollection(this);
        }

        private void Apply(INameSetEvent @event)
        {
            Name = @event.Name;
        }

        public string Name { get; private set; }

        private Bar.CollectionManager _bars;

        public void SetName(Guid id, string name)
        {
            if (name.Length < 3)
                throw new ArgumentException("Name should be at least 5 characters.");

            Publish(new NameSet(id, name));
        }
    }
}