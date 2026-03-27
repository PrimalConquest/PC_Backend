using SimulationEngine.Events.Payloads;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimulationEngine.Events.Busses.Interfaces
{
    internal interface IEventBus<T> : IBus<T, EventPayload> { }
}
