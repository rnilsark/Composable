using System.Runtime.Serialization;
using Common.CQRS;

namespace Actor1.Interfaces.Commands
{
    [DataContract]
    public class SetNameCommand : CommandBase
    {
        [DataMember]
        public string Name { get; set; }
    }
}
