using System.Runtime.Serialization;
using Common.CQRS;

namespace Actor1.Interfaces.Commands
{
    [DataContract]
    public class CreateCommand : CommandBase
    {
        [DataMember]
        public string Name { get; set; }
    }
    
    [DataContract]
    public class RenameCommand : CommandBase
    {
        [DataMember]
        public string Name { get; set; }
    }
}
