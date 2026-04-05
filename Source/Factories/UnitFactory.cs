using SimulationEngine.Source.Data.Abilities;
using SimulationEngine.Source.Data.Geometry;
using SimulationEngine.Source.Data.Units;
using SimulationEngine.Source.Enums.EventTypes;
using SimulationEngine.Source.Enums.Logging;
using SimulationEngine.Source.Helpers.Units;
using SimulationEngine.Source.Systems;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimulationEngine.Source.Factories
{
    internal static class UnitFactory
    {
        static Dictionary<string, Unit> _parsedUnits = new();

        public static Unit? GetUnit(string unitId)
        {
            if (_parsedUnits.ContainsKey(unitId))
            {
                return _parsedUnits[unitId].DeepCopy();
            }

            UnitData? data = UnitHelper.Parse(unitId);
            if (data == null)
            {
                LogSystem.Log(ELogCategory.Debug, ELogLevel.Warning, $"UnitFactory.GetUnit");
                return null;
            }



            //_parsedUnits.Add(unitId, new());
            return null;
        }

        static Dictionary<EUnitEvent, Ability> GetAbilitymap(Dictionary<string, string> jsonMap)
        {
            return new();
        }
    }
}
