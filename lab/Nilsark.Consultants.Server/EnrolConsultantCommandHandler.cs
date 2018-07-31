using Composable.Messaging.Buses;
using Nilsark.Consultants.Server.Domain;
using Nilsark.Consultants.Shared.Commands;

namespace Nilsark.Consultants.Server
{
    public class EnrolConsultantCommandHandler
    {
        public static void Register(MessageHandlerRegistrarWithDependencyInjectionSupport registrar) =>
            registrar
                .ForCommandWithResult((EnrolConsultantCommand cmd, ILocalApiNavigatorSession bus) => Handle(cmd, bus));

        private static CommandResult Handle(EnrolConsultantCommand cmd, ILocalApiNavigatorSession bus)
        {
            Consultant.Create(cmd.ConsultantId, cmd.FullName, cmd.Email, bus);
            return CommandResult.Success();
        }
    }
}