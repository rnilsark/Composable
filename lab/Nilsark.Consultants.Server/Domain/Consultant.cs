using System;
using System.Diagnostics.Contracts;
using Composable;
using Composable.GenericAbstractions.Time;
using Composable.Messaging.Buses;
using Composable.Persistence.EventStore.Aggregates;
using Nilsark.Consultants.Shared.Domain.Events;

namespace Nilsark.Consultants.Server.Domain
{
    internal class Consultant : Aggregate<Consultant, ConsultantEvent, IConsultantEvent>
    {
        private static ComposableApi ComposableApi => new ComposableApi();

        private Consultant() : base(new DateTimeNowTimeSource())
        {
            RegisterEventAppliers()
                .For<IConsultantFullNamePropertyUpdated>(e => FullName = e.FullName)
                .For<IConsultantEmailPropertyUpdated>(e => Email = e.Email)
                .IgnoreUnhandled<IConsultantEvent>();
        }

        public string FullName { get; private set; }

        public string Email { get; private set; }

        internal static void Create(Guid aggregateId, string fullName, string email, ILocalApiNavigatorSession bus)
        {
            var consultant = new Consultant();
            consultant.Publish(new ConsultantEnrolled(aggregateId, fullName, email));
            bus.Execute(ComposableApi.EventStore.Commands.Save(consultant));
        }

        internal void Dismiss()
        {
            Publish(new ConsultantDismissed(Id));
        }

        protected override void AssertInvariantsAreMet()
        {
            base.AssertInvariantsAreMet();

            Contract.Assert(FullName.Length >= 3);
        }
    }
}