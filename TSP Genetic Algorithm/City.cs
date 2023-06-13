using System.Windows;

namespace TSP_Genetic_Algorithm
{
    public class City
    {
        public string Name { get; set; }
        public Point Location { get; set; }

        public override string ToString()
        {
            return $"{Name} ({Location.X}, {Location.Y})";
        }
    }
}
