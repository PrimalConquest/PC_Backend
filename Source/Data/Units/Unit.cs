using SimulationEngine.Source.Data.Stats;
using SimulationEngine.Source.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimulationEngine.Source.Data.Units
{
    internal abstract class Unit
    {
        StatSheet _stats;
        public uint Id { get; private set; }
        public EColor Color { get; private set; }

        //Add Activate Ability
        
        protected Unit(uint id)
        {
            Id = id;

            //if(color )
            
            _stats = new();
            /*Stat<ushort> Health = new();
            foreach (EValueType type in Enum.GetValues(typeof(EValueType))) Health.RegisterValue(EValueType.BASE);
            _stats.RegisterAttribute(EStat.Health, Health);*/
        }
    }
}
