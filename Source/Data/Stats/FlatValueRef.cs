using SimulationEngine.Source.Data.Units;
using SimulationEngine.Source.Enums.Stats;
using SimulationEngine.Source.Interfaces;

namespace SimulationEngine.Source.Data.Stats
{
    public class FlatValueRef : IValueRef
    {
        readonly int _value;

        public FlatValueRef(int value)
        {
            _value = value;
        }

        public int Resolve(EStat stat, Unit owner, Unit target) => _value;

        public IValueRef DeepCopy() => new FlatValueRef(_value);
    }
}
