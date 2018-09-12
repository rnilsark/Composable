using Domain.Events;
using Domain.Events.Implementation;

namespace Domain
{
    public partial class Foo
    {
        public class Bar : Entity<Bar, int, BarEvent, IBarEvent, IBarAdded, BarEvent.IdGetterSetter>
        {
            public Bar(Foo aggregateRoot, int id) : base(aggregateRoot)
            {
                RegisterEventAppliers();
            }
        }
    }
}