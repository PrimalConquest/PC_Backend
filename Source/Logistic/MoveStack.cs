using SimulationEngine.Source.Data.Geometry;
using SimulationEngine.Source.Data.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimulationEngine.Source.Logistic
{
    internal class MoveStack
    {
        public List<List<(Unit unit, Cell destination)>> moves;
        public uint moveCost;

        public MoveStack()
        {
            moves = new();
            Invalidate();
        }

        public void Invalidate()
        {
            moves.Clear();
            moveCost = 0;
        }

        public void AddTimeStep(List<(Unit unit, Cell destination)> gatherList)
        {
            moves.Add(gatherList);
        }

        public void NextTimeStep()
        {
            moves.Add(new());
        }

        public void AddMoveInCurrentTimeStep(Unit unit, Cell destination)
        {
            moves.Last().Add((unit, destination));
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"MoveStack [MoveCost: {moveCost}]");

            for (int i = 0; i < moves.Count; i++)
            {
                sb.AppendLine($"  TimeStep {i}:");
                if (moves[i].Count == 0)
                {
                    sb.AppendLine("    (empty)");
                }
                else
                {
                    foreach (var (unit, destination) in moves[i])
                    {
                        sb.AppendLine($"    [{unit}]{unit.Position} -> {destination}");
                    }
                }
            }

            return sb.ToString();
        }
    }
}
