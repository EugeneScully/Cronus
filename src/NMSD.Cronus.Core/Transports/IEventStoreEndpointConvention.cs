using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NMSD.Cronus.Core.Transports.Conventions
{
    public interface IEventStoreEndpointConvention
    {
        IEnumerable<EndpointDefinition> GetEndpointDefinitions(Assembly assemblyContainingEvents);
    }
}
