using SimulationEngine.Source.Data.Geometry;
using System.Collections.Generic;

namespace SimulationEngine.Source.Data.Geometry
{
    public struct Shape
    {
        /// <summary>
        /// Anchor = top-left tile of the shape (lowest y, then lowest x).
        /// Width  = number of columns (extends in +x direction from anchor).
        /// Height = number of rows    (extends in +y direction from anchor, y increases downward).
        /// </summary>
        public int Width  { get; set; }
        public int Height { get; set; }

        public Shape()
        {
            Width  = 1;
            Height = 1;
        }

        public Shape(int width, int height)
        {
            Width  = width;
            Height = height;
        }

        /// <summary>
        /// Enumerates all cell offsets relative to the anchor (top-left = {x:0, y:0}).
        /// dx in [0, Width), dy in [0, Height).  y increases downward.
        /// </summary>
        public IEnumerable<Cell> GetOffsets()
        {
            for (int dy = 0; dy < Height; dy++)
                for (int dx = 0; dx < Width; dx++)
                    yield return new Cell { x = dx, y = dy };
        }
    }
}
