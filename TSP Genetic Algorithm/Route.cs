using System;
using System.Collections.Generic;
using System.Linq;

namespace TSP_Genetic_Algorithm
{
    public class Route
    {
        public List<City> Cities { get; set; }

        // Calculates the total distance of the route
        public double CalculateDistance()
        {
            double totalDistance = 0;
            for (int i = 0; i < Cities.Count - 1; i++)
            {
                totalDistance += Distance(Cities[i], Cities[i + 1]);
            }

            // return to the starting city
            totalDistance += Distance(Cities[Cities.Count - 1], Cities[0]);
            return totalDistance;
        }

        private double Distance(City city1, City city2)
        {
            double xDistance = Math.Abs(city1.Location.X - city2.Location.X);
            double yDistance = Math.Abs(city1.Location.Y - city2.Location.Y);
            return Math.Sqrt(xDistance * xDistance + yDistance * yDistance);
        }
    }

    public class Population
    {
        public List<Route> Routes { get; set; }

        // Calculates the average fitness of the population
        public double AverageFitness()
        {
            return Routes.Sum(route => 1.0 / route.CalculateDistance()) / Routes.Count;
        }

        // Returns the best route in the population
        public Route BestRoute()
        {
            return Routes.OrderBy(route => route.CalculateDistance()).First();
        }
    }

}
