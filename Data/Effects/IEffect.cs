
using System;
using System.Collections.Generic;
using System.Text;

namespace SimulationEngine.Data.Effects
{
    internal interface IEffect
    {
        void Apply(Object intigator, Object reciever);
    }
}
