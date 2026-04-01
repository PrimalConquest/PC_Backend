using SimulationEngine.Source.Events.Payloads;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimulationEngine.Source.Interfaces
{
    internal interface IAbility
    {
        void Init(Object gameState);

        void Activate(EventPayload payload); 
    }
}
