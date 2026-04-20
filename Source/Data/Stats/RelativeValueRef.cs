using SimulationEngine.Source.Data.Units;
using SimulationEngine.Source.Enums.Stats;
using SimulationEngine.Source.Interfaces;

namespace SimulationEngine.Source.Data.Stats
{

    public class RelativeValueRef : IValueRef
    {
        readonly EValueSource _source;

        public RelativeValueRef(EValueSource source)
        {
            _source = source;
        }

        public int Resolve(EStat stat, Unit owner, Unit target)
        {

            switch(_source)
            {
                case EValueSource.Owner: return owner.GetStat(stat);
                case EValueSource.Target: return target.GetStat(stat);
                default: return 0;
            }
        }

        public IValueRef DeepCopy() => new RelativeValueRef(_source);
    }
}
