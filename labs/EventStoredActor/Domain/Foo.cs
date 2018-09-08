using System;
using System.Collections.Generic;
using System.Linq;
using Common.DDD;
using Domain.Events;
using Domain.Events.Implementation;

namespace Domain
{
    public partial class Foo : AggregateRoot<IFooEvent>
    {
        public Foo()
        {
            RegisterEventAppliers()
                .For<IFooCreatedEvent>(Apply)
                .For<IFooNamePropertyUpdated>(e => Name = e.Name)
                .For<IBarAdded>(e => Bars.Add(new Bar(this, e.BarId)))
                .For<IBarEvent>(e => Bars.Single(bar => bar.Id == e.BarId).ApplyEvent(e));
        }

        private void Apply(IFooCreatedEvent @event)
        {
            Name = @event.Name;
        }

        public string Name { get; set; }
        public IList<Bar> Bars { get; set; } = new List<Bar>();

        public void SetName(Guid id, string name)
        {
            if (name.Length < 3)
                throw new ArgumentException("Name should be at least 5 characters.");

            RaiseEvent(new NameSet
            {
                AggregateRootId = id,
                Name = name
            });
        }
    }
}