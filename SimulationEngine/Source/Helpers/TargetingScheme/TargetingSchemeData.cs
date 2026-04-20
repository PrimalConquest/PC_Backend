using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimulationEngine.Source.Helpers.TargetingScheme
{
    internal class TargetingSchemeData
    {
        public string Class { get; set; } = "";
        public JObject Specific { get; set; } = new();
    }
}
