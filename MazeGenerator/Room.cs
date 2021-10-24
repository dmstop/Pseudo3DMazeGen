using System.Collections.Generic;
using MazeGenerator.Enum;

namespace MazeGenerator
{
    public class Room
    {
        public int X { get; set; }

        public int Y { get; set; }

        public List<Direction> Links { get; set; } = new List<Direction>();
    }
}
