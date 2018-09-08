using Domain.Events;

namespace Domain
{
    public partial class Foo
    {
        public class Bar : Entity<Foo, IBarEvent>
        {
            public Bar(Foo aggregateRoot, int id) : base(aggregateRoot)
            {
                Id = id;
                RegisterEventAppliers();
            }

            public int Id { get; }
        }
    }
}