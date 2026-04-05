using System;
using System.Collections.Generic;
using System.Text;

namespace SimulationEngine.Source.Interfaces
{
    public interface IDeepCopyable<T>
    {
        T DeepCopy();
    }
}
