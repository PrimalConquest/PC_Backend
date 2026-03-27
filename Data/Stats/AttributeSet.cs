using SimulationEngine.Data.Stats.Enums;
using SimulationEngine.Events.Busses;
using SimulationEngine.Events.Busses.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimulationEngine.Data.Stats
{
    internal class AttributeSet
    {
        private Dictionary<EStat, Attribute> _attributes;
        private IEventBus<EStat> _onStatChanged;

        public AttributeSet()
        {
            _attributes = new();
            _onStatChanged = new PriorityEventBus<EStat>();
        }
        public AttributeSet(IEventBus<EStat> onValueChangedBus)
        {
            _attributes = new();
            _onStatChanged = onValueChangedBus;
        }

        public void RegisterAttribute(EStat stat, Attribute attribute)
        {
            _attributes.Add(stat, attribute);
        }

        public Attribute GetAttribute(EStat stat)
        {
            _attributes.TryGetValue(stat, out var attribute);
            return attribute;
        } 
    }
}
