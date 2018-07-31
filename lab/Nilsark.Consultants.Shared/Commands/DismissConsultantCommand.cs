using System;
using System.ComponentModel.DataAnnotations;
using Composable.Messaging;
using Newtonsoft.Json;

namespace Nilsark.Consultants.Shared.Commands
{
    public class DismissConsultantCommand : BusApi.Remotable.ExactlyOnce.Command
    {
        private DismissConsultantCommand() : base()
        {
        }

        public static DismissConsultantCommand Create(Guid consultantId)
        {
            return new DismissConsultantCommand
            {
                ConsultantId = consultantId,
            };
        }

        [Required]
        [JsonProperty]
        public Guid ConsultantId { get; private set; }
    }
}