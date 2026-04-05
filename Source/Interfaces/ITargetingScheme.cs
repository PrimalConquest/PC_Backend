using Newtonsoft.Json.Linq;
using SimulationEngine.Source.Data.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimulationEngine.Source.Interfaces
{
    public interface ITargetingScheme : IDeepCopyable<ITargetingScheme>
    {
        List<Unit> GatherTargets(Unit referenceUnit);
        void Extract(JObject spec);
    }
}
