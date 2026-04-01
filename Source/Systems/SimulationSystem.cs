using System;
using System.Collections.Generic;
using System.Text;

namespace SimulationEngine.Source.Systems
{
    internal static class SimulationSystem
    {
        private static Random _rng = new Random();

        private static uint _currentId = 0;

        public static int Seed 
        {
            get;
            set
            {
                field = value;
                _rng = new Random(field);
            }
        }

        public static int RandomInt() => _rng.Next();
        public static uint NextId() => ++_currentId;

        public static void Init(int seed, uint currentId)
        {
            Seed = seed;
            _currentId = currentId;
        }

        static SimulationSystem()
        {
            Seed = new Random().Next();
        }
    }
}
