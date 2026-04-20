using Newtonsoft.Json.Linq;
using SimulationEngine.Source.Data.Units;
using SimulationEngine.Source.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimulationEngine.Source.Data.TargetingSchemes
{
    internal class TargetSelf : ITargetingScheme
    {
        public void Extract(JObject spec)
        {
            return;
        }

        public List<Unit> GatherTargets(Unit referenceUnit)
        {
            return new([referenceUnit]);
        }
    }
}
