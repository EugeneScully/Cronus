﻿using Elders.Cronus.MessageProcessing;

namespace Elders.Cronus.Pipeline.CircuitBreaker
{
    public interface IEndpointCircuitBreaker
    {
        /// <summary>
        /// Set the maximum delivery count for messages in the queue. A message is automatically 
        /// dead-lettered after this number of deliveries. The default value for dead letter count is 5.
        /// </summary>
        /// <value>The maximum message age.</value>
        int MaximumMessageAge { get; }
        void PostConsume(IFeedResult mesages);
    }
}