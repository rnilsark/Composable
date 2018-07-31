using System;
using Composable.Persistence.EventStore;

namespace Nilsark.Consultants.Shared.Domain.Events
{
    public abstract class ConsultantEvent : AggregateEvent, IConsultantEvent
    {
        protected ConsultantEvent(Guid aggregateId) : base(aggregateId)
        {
        }
    }

    public class ConsultantEnrolled : ConsultantEvent, IConsultantEnrolledEvent
    {
        public ConsultantEnrolled(Guid aggregateId, string fullName, string email)
            : base(aggregateId)
        {
            FullName = fullName;
            Email = email;
        }

        public string FullName { get; }

        public string Email { get; }
    }

    public class ConsultantDismissed : ConsultantEvent, IConsultantDismissedEvent
    {
        public ConsultantDismissed(Guid aggregateId)
            : base(aggregateId)
        {
        }
    }
}