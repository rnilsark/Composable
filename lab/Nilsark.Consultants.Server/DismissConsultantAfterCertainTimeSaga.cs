using Composable.Messaging.Buses;
using Nilsark.Consultants.Shared.Commands;
using Nilsark.Consultants.Shared.Domain.Events;

namespace Nilsark.Consultants.Server
{
    public class DismissConsultantAfterCertainTimeSaga
    {
        public static void Register(MessageHandlerRegistrarWithDependencyInjectionSupport registrar) =>
            registrar.ForEvent((IConsultantEnrolledEvent e, IServiceBusSession bus) => Handle(e, bus));

        private static void Handle(IConsultantEnrolledEvent e, IIntegrationBusSession bus) => bus.ScheduleSend(e.UtcTimeStamp.AddSeconds(30), DismissConsultantCommand.Create(e.AggregateId));
    }
}