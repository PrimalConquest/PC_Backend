using SimulationEngine.Source.Data.Stats;
using SimulationEngine.Source.Enums;
using SimulationEngine.Source.Enums.EventTypes;
using SimulationEngine.Source.Enums.Logging;
using SimulationEngine.Source.Enums.Stats;
using SimulationEngine.Source.Events.Busses;
using SimulationEngine.Source.Events.Payloads;
using SimulationEngine.Source.Interfaces.Events;
using SimulationEngine.Source.Systems;
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

        private IEventBus<EUnitEvent, EventPayload> _internalEventBus;

        //Add Activate Ability

        protected Unit(uint id)
        {
            Id = id;
            _internalEventBus = new PriorityEventBus<EUnitEvent, EventPayload>();
            //if(color )

            _stats = new();
            /*Stat<ushort> Health = new();
            foreach (EValueType type in Enum.GetValues(typeof(EValueType))) Health.RegisterValue(EValueType.BASE);
            _stats.RegisterAttribute(EStat.Health, Health);*/

            _internalEventBus.AddListener(EUnitEvent.GetStat, new(OnGetStat));
            
        }

        private void OnGetStat(EventPayload payload)
        {
            StatPayload? statPayload = payload as StatPayload;
            if(statPayload == null)
            {
                LogSystem.Log(ELogCategory.Debug, ELogLevel.Warning, "Unit.OnGetStat - payload is not of type StatPayload");
                return;
            }

             statPayload.Value = _stats.GetStat(statPayload.Stat);
        }

        public void Trigger(EUnitEvent signal, EventPayload data)
        {
            _internalEventBus.Raise(signal, data);
        }
    }
}
