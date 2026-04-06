using SimulationEngine.Source.Data.Units;
using SimulationEngine.Source.Factories;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimulationEngine.Source.Logistic
{
    public class Player
    {
        uint _id;
        Board _board;
        Dictionary<uint, Unit> _commanders;

        Dictionary<uint, Unit> _units;

        public Player(uint id)
        {
            _id = id;
        }
    }
}
