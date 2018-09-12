using System;
using Composable.GenericAbstractions.Time;
using Composable.Persistence.EventStore;
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
                .For<IRenamedEvent>(Apply)
                .For<IFooNamePropertyUpdated>(e => Name = e.Name);

            _bars = Bar.CreateSelfManagingCollection(this);
        }

        private void Apply(IRenamedEvent @event)
        {
            Name = @event.Name;
        }

        public string Name { get; private set; }

        private Bar.CollectionManager _bars;

        public static void Create(Guid id, string name, IEventStoreUpdater eventStoreUpdater)
        {
            var foo = new Foo();
            foo.Publish(new CreatedEvent(id, name));
            eventStoreUpdater.Save(foo);
        }

        public void Rename(string name)
        {
            if (name.Length < 3)
                throw new ArgumentException("Name should be at least 5 characters.");

            Publish(new RenamedEvent(name));
        }
    }
}