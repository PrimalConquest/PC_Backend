using SimulationEngine.Source.Data.Geometry;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimulationEngine.Source.Data.Units
{
    internal class FieldUnit : Unit
    {
        Point _position;
        public int X { get { return _position.X; } set { _position.X = value; } }
        public int Y { get { return _position.Y; } set { _position.Y = value; } }

        Shape _extend;
        public FieldUnit(uint id) : base(id)
        {
            
        }
    }
}
