using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimulationEngine.Source.Data.Units;
using SimulationEngine.Source.Enums.Stats;
using SimulationEngine.Source.Events.Payloads;
using SimulationEngine.Source.Helpers;
using SimulationEngine.Source.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimulationEngine.Source.Data.Abilities
{
    internal class GrantStat : Ability
    {
        EStat _stat;
        int _value;

        public GrantStat(Unit owner, int priority = 5, ITargetingScheme? targetingScheme = null) : base(owner, priority, targetingScheme)
        {
        }

        public override void Activate(EventPayload payload)
        {
            List<Unit> targets = GetTargets();

            foreach(Unit target in targets)
            {
                target.Stats.GrantStat(_stat, _value);
            }

        }

        public override Ability DeepCopy()
        {
            GrantStat ability = new(Owner);
            ability._stat = _stat;
            ability._value = _value;
            return ability;
        }

        public override void Extract(JObject spec)
        {
            foreach (var prop in spec.Properties())
            {
                EStat? s = StatHelper.ToStat(prop.Name);
                if (s != null)
                {
                    _stat = s.Value;
                    _value = prop.Value.Value<int>();
                    break;
                }
            }
        }
    }
}
