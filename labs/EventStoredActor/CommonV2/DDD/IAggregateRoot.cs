using Common.DDD;

namespace CommonV2.DDD
{
    public interface IAggregateRoot
    {
        IEventController EventController { set; }
    }
}