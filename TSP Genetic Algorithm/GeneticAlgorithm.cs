using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
            Population newPopulation = new Population { Routes = new List<Route>() };
            for (int i = 0; i < population.Routes.Count; i++)
            {
                Route parent1 = TournamentSelection(population, tournamentSize);
                Route parent2 = TournamentSelection(population, tournamentSize);
                Route child = OrderCrossover(parent1, parent2);

                if (random.NextDouble() < mutationRate)
                {
                    Mutate(child);
                }

                newPopulation.Routes.Add(child);
            }

            return newPopulation;
        }

        public Route RunGeneticAlgorithm(Population initialPopulation, int generations, int tournamentSize, double mutationRate)
        {
            Population population = initialPopulation;

            for (int i = 0; i < generations; i++)
            {
                population = Evolve(population, tournamentSize, mutationRate);
            }

            return population.BestRoute();
        }

        public async Task<Route> RunGeneticAlgorithmParallel(Population initialPopulation, int generations, int tournamentSize, double mutationRate, int numberOfThreads)
        {
            Population population = initialPopulation;
            SemaphoreSlim semaphore = new SemaphoreSlim(numberOfThreads, numberOfThreads);

            for (int i = 0; i < generations; i++)
            {
                List<Task<Route>> tasks = new List<Task<Route>>(numberOfThreads);

                for (int j = 0; j < population.Routes.Count; j++)
                {
                    await semaphore.WaitAsync();

                    Task<Route> task = Task.Run(() =>
                    {
                        Route parent1 = TournamentSelection(population, tournamentSize);
                        Route parent2 = TournamentSelection(population, tournamentSize);
                        Route child = OrderCrossover(parent1, parent2);

                        if (random.NextDouble() < mutationRate)
                        {
                            Mutate(child);
                        }

                        semaphore.Release();
                        return child;
                    });
                    tasks.Add(task);
                }

                Route[] newRoutes = await Task.WhenAll(tasks);
                population.Routes = newRoutes.ToList();
            }
            return population.BestRoute();
        }
    }
}
