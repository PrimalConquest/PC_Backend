using SimulationEngine.Source.Data.Units;
using SimulationEngine.Source.Enums.Stats;

namespace SimulationEngine.Source.Interfaces
{
    public interface IValueRef : IDeepCopyable<IValueRef>
    {
        public int Resolve(EStat stat, Unit owner, Unit target);
    }
}
