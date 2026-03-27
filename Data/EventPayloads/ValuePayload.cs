using SimulationEngine.Events.Payloads;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimulationEngine.Data.EventPayloads
{
    internal class ValuePayload<T> : EventPayload
    {
        public T Value { get; set; }

        public ValuePayload(T value)
        {
            Value = value;
        }
    }
}
