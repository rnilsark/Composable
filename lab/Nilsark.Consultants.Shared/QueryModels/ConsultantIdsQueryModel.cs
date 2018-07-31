using System;
using System.Collections.Generic;
using Composable.DDD;

namespace Nilsark.Consultants.Shared.QueryModels
{
    public class ConsultantIdsQueryModel : IHasPersistentIdentity<Guid>
    {
        public ConsultantIdsQueryModel(Guid id)
        {
            Id = id;
        }

        public HashSet<Guid> ConsultantIds { get; internal set; } = new HashSet<Guid>();
        
        public Guid Id { get; }
    }
}
