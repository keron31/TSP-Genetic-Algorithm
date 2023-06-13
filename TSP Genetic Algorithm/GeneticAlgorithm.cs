using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TSP_Genetic_Algorithm
{
    public class GeneticAlgorithm
    {
        private Random random = new Random();

        public Route TournamentSelection(Population population, int tournamentSize)
        {
            List<Route> tournament = new List<Route>();
            for (int i = 0; i < tournamentSize; i++)
            {
                int randomId = random.Next(0, population.Routes.Count);
                tournament.Add(population.Routes[randomId]);
            }

            return tournament.OrderBy(route => route.CalculateDistance()).First();
        }

        public Route OrderCrossover(Route parent1, Route parent2)
        {
            Route child = new Route { Cities = new List<City>() };
            int startPos = random.Next(parent1.Cities.Count);
            int endPos = random.Next(startPos, parent1.Cities.Count);

            List<City> childPart = parent1.Cities.GetRange(startPos, endPos - startPos);
            child.Cities.AddRange(childPart);

            foreach (City city in parent2.Cities)
            {
                if (!child.Cities.Contains(city))
                {
                    child.Cities.Add(city);
                }
            }

            return child;
        }

        public void Mutate(Route route)
        {
            for (int pos1 = random.Next(route.Cities.Count); pos1 < route.Cities.Count; pos1++)
            {
                int pos2 = random.Next(route.Cities.Count);

                City city1 = route.Cities[pos1];
                City city2 = route.Cities[pos2];

                route.Cities[pos2] = city1;
                route.Cities[pos1] = city2;
            }
        }

        public Population Evolve(Population population, int tournamentSize, double mutationRate)
        {
            Population newPopulation = new Population();
            for (int i = 0; i < population.Routes.Count; i++)
            {
                Route parent1 = TournamentSelection(population, tournamentSize);
                Route parent2 = TournamentSelection(population, tournamentSize);
                Route child = OrderCrossover(parent1, parent2);

                // Mutate the route
                if (random.NextDouble() < mutationRate)
                {
                    Mutate(child);
                }

                newPopulation.Routes.Add(child);
            }

            return newPopulation;
        }

        public void RunGeneticAlgorithm(Population initialPopulation, int generations, int tournamentSize, double mutationRate)
        {
            Population population = initialPopulation;

            for (int i = 0; i < generations; i++)
            {
                population = Evolve(population, tournamentSize, mutationRate);
            }

            Route bestRoute = population.BestRoute();
            // Display or return the best route
        }

        public async Task<Route> RunGeneticAlgorithmParallel(Population initialPopulation, int generations, int tournamentSize, double mutationRate, int numberOfThreads)
        {
            Population population = initialPopulation;
            BlockingCollection<Route> newPopulation = new BlockingCollection<Route>();

            // This is the "master" task
            var masterTask = Task.Run(async () =>
            {
                for (int i = 0; i < generations; i++)
                {
                    newPopulation = new BlockingCollection<Route>();

                    for (int j = 0; j < population.Routes.Count; j += numberOfThreads)
                    {
                        // Wait for the workers to generate the new individuals
                        for (int k = 0; k < numberOfThreads && j + k < population.Routes.Count; k++)
                        {
                            newPopulation.Add(await Task.Run(() =>
                            {
                                Route parent1 = TournamentSelection(population, tournamentSize);
                                Route parent2 = TournamentSelection(population, tournamentSize);
                                Route child = OrderCrossover(parent1, parent2);

                                // Mutate the route
                                if (random.NextDouble() < mutationRate)
                                {
                                    Mutate(child);
                                }

                                return child;
                            }));
                        }
                    }

                    // Replace the old population with the new one
                    population.Routes = new List<Route>(newPopulation);
                }
            });

            await masterTask;
            return population.BestRoute();
        }

    }

}
