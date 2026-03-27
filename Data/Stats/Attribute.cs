using SimulationEngine.Data.EventPayloads;
using SimulationEngine.Data.Stats.Enums;
using SimulationEngine.Events;
using SimulationEngine.Events.Busses;
using SimulationEngine.Events.Busses.Interfaces;
using SimulationEngine.Events.Payloads;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimulationEngine.Data.Stats
{
    internal class Attribute<T>
    {
        private Dictionary<EValueType, T> _values;
        private IEventBus<EValueType> _onValueChanged;
        private IEventBus<EValueType> _onGetValue;

        public Attribute()
        {
            _values = new();
            _onValueChanged = new PriorityEventBus<EValueType>();
            _onGetValue = new PriorityEventBus<EValueType>();
        }
        public Attribute(IEventBus<EValueType> onValueChangedBus, IEventBus<EValueType> onGetValue)
        {
            _values = new();
            _onValueChanged = onValueChangedBus;
            _onGetValue = onGetValue;
        }

        public Attribute(T baseValue) : this()
        {
            RegisterValue(EValueType.BASE, baseValue);
        }

        public Attribute(T baseValue, T currentValue) : this(baseValue)
        {
            RegisterValue(EValueType.CURRENT, currentValue);
        }

        public Attribute(T baseValue, T currentValue, T maxValue) : this(baseValue, currentValue)
        {
            RegisterValue(EValueType.MAX, maxValue);
        }

        public Attribute(T baseValue, T currentValue, T maxValue, T minValue) : this(baseValue, currentValue, maxValue)
        {
            RegisterValue(EValueType.MIN, minValue);
        }

        public void RegisterValue(EValueType type, T value)
        {
            SetValue(type, value);
            _onValueChanged.RegisterChannel(type);
        }

        public void SetValue(EValueType type, T value)
        {
            _values.Add(type, value);
        }

        public T GetValue(EValueType type)
        {
            _values.TryGetValue(type, out T value);

            ValuePayload<T> payload = new ValuePayload<T>(value);

            _onGetValue.Raise(type, payload);

            return payload.Value;
        }

        public bool ListenOnValueChange(EValueType type, EventCallback<EventPayload> callback, int priority = 0, bool enforceEventCreation = false)
        {
            if(!_values.ContainsKey(type)) return false;

            _onValueChanged.AddListener(type, callback, priority, enforceEventCreation);

            return true;
        }

        public bool StopListenOnValueChange(EValueType type, EventCallback<EventPayload> callback)
        {
            if (_values.ContainsKey(type))
            { 
                _onValueChanged.RemoveListener(type, callback);
            }
            return true;
        }

        public bool ClearListenersOnValueChange(EValueType type)
        {
            if (_values.ContainsKey(type))
            {
                _onValueChanged.ClearChannel(type);
            }
            return true;
        }

    }
}
