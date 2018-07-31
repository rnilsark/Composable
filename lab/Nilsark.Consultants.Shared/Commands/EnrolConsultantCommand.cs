using System;
using System.ComponentModel.DataAnnotations;
using Composable.Messaging;
using Newtonsoft.Json;

namespace Nilsark.Consultants.Shared.Commands
{
    public class EnrolConsultantCommand : BusApi.Remotable.AtMostOnce.Command<CommandResult>
    {
        public EnrolConsultantCommand() : base(DeduplicationIdHandling.Reuse)
        {
        }

        public static EnrolConsultantCommand Create(Guid consultantId, string fullName, string email)
        {
            return new EnrolConsultantCommand
            {
                DeduplicationId = Guid.NewGuid(),
                ConsultantId = consultantId,
                FullName = fullName,
                Email =  email
            };
        }

        [Required]
        [JsonProperty]
        public Guid ConsultantId { get; private set; }

        [MinLength(3)]
        [Required]
        [JsonProperty]
        public string FullName { get; private set; }

        [Required]
        [JsonProperty]
        public string Email { get; private set; }
    }
}