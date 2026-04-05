using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimulationEngine.Source.Helpers.Abilities
{
    internal class AbilityData
    {
        public string Class { get; set; } = "";
        public int ActivationCost { get; set; } = 0;
        public int Priority { get; set; } = 5;
        public string TargetingId { get; set; } = "";
        public JObject Specific { get; set; } = new();
    }
}
