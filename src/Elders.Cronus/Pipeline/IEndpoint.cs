using System;
using System.Collections.Generic;

namespace Elders.Cronus.Pipeline
{
    public interface IEndpoint : IEquatable<IEndpoint>
    {
        IDictionary<string, object> RoutingHeaders { get; set; }

        string RoutingKey { get; }

        string Name { get; }

        bool BlockDequeue(uint timeoutInMiliseconds, out EndpointMessage msg);

        EndpointMessage DequeueNoWait();

        void Open();

        void Close();

        void Acknowledge(EndpointMessage message);

        void AcknowledgeAll();
    }
}