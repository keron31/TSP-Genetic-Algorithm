using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TSP_Genetic_Algorithm
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TxtCities_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnAddCities.IsEnabled = !string.IsNullOrWhiteSpace(txtCities.Text);
        }

        private void Parameters_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnStart.IsEnabled = !string.IsNullOrWhiteSpace(txtThreads.Text) &&
                                 !string.IsNullOrWhiteSpace(txtPopulationSize.Text) &&
                                 !string.IsNullOrWhiteSpace(txtMutationRate.Text);
        }


        private void AddCities_Click(object sender, RoutedEventArgs e)
        {
            string[] lines = txtCities.Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 3)
                {
                    txtResults.Text = "Miasta i koordynaty należy wprowadzić każde w osobnej linni oddzielone od siebie spacją.";
                    return;
                }

                string name = parts[0];
                double x, y;
                if (!double.TryParse(parts[1], out x) || !double.TryParse(parts[2], out y))
                {
                    txtResults.Text = "Szerokość i długość geograficzna musi być liczbą zmiennoprzecinkową.";
                    return;
                }

                var city = new City { Name = name, Location = new Point(x, y) };
                lstCities.Items.Add(city);
            }
            txtCities.Clear();

        }

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            txtResults.Clear();
            // Parse the user input
            if (!int.TryParse(txtThreads.Text, out int numberOfThreads))
            {
                txtResults.Text = "Liczba wątków musi być liczbą całkowitą.";
                return;
            }
            if (!int.TryParse(txtPopulationSize.Text, out int populationSize))
            {
                txtResults.Text = "Rozmiar populacji musi być liczbą całkowitą.";
                return;
            }
            if (!double.TryParse(txtMutationRate.Text, out double mutationRate))
            {
                txtResults.Text = "Prawdopodobieństwo mutacji musi być liczbą zmiennoprzecinkową.";
                return;
            }
            if (numberOfThreads > Environment.ProcessorCount)
            {
                txtResults.Text = $"Liczba wątków nie może przekraczać liczby dostępnych procesorów ({Environment.ProcessorCount}).";
                return;
            }

            // Create the initial population
            List<Route> initialRoutes = new List<Route>();
            for (int i = 0; i < populationSize; i++)
            {
                initialRoutes.Add(new Route { Cities = new List<City>(lstCities.Items.Cast<City>()) });
            }
            Population initialPopulation = new Population { Routes = initialRoutes };

            // Create the genetic algorithm
            GeneticAlgorithm ga = new GeneticAlgorithm();

            Stopwatch stopwatch = Stopwatch.StartNew(); // start timing

            progressBar.Visibility = Visibility.Visible; // show the progress bar
            progressBar.IsIndeterminate = true; // set the progress bar to indeterminate mode

            // Run the genetic algorithm
            Route bestRoute = await ga.RunGeneticAlgorithmParallel(initialPopulation, 1000, 5, mutationRate, numberOfThreads);

            stopwatch.Stop(); // stop timing

            progressBar.Visibility = Visibility.Hidden; // hide the progress bar

            // Display the best route
            txtResults.Text = $"Najlepsza trasa: {bestRoute.CalculateDistance()} \nCzas wykonywania: {stopwatch.Elapsed.TotalSeconds} sekund";
        }

        private void RemoveCity_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var city = (City)button.DataContext;
            lstCities.Items.Remove(city);
        }
    }
}
