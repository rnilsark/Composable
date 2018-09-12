using System;
using Common.DDD;
using CommonV2.DDD;
using Composable.Persistence.EventStore;
using Domain.Events;
using Domain.Events.Implementation;

namespace Domain
{
    public partial class Foo : AggregateRoot<Foo, FooEvent, IFooEvent>
    {
        public Foo()
        {
            RegisterEventAppliers()
                .For<IRenamedEvent>(Apply)
                .For<IFooNamePropertyUpdated>(e => Name = e.Name);

            _bars = Bar.CreateSelfManagingCollection(this); //This is different from FG.CQRS, but cleaner.
        }

        private Foo(IEventController eventController) : this()
        {
            EventController = eventController;
        }

        private void Apply(IRenamedEvent @event)
        {
            Name = @event.Name;
        }

        public string Name { get; private set; }

        private Bar.CollectionManager _bars;

        public static void Create(Guid id, string name, IEventStoreUpdater eventStoreUpdater, IEventController eventController)
        {
            var foo = new Foo(eventController);
            foo.RaiseEvent(new CreatedEvent(id, name));
            eventStoreUpdater.Save(foo);
        }

        public void Rename(string name)
        {
            if (name.Length < 3)
                throw new ArgumentException("Name should be at least 5 characters.");

            RaiseEvent(new RenamedEvent(name));
        }
    }
}