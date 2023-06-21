using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TSP_Genetic_Algorithm
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void RadioSingleThreaded_Checked(object sender, RoutedEventArgs e)
        {
            txtThreads.Clear();
            txtThreads.IsEnabled = false;
            Parameters_TextChanged(sender, null);
        }

        private void RadioSingleThreaded_Unchecked(object sender, RoutedEventArgs e)
        {
            txtThreads.IsEnabled = true;
            Parameters_TextChanged(sender, null);
        }

        private void TxtCities_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnAddCities.IsEnabled = !string.IsNullOrWhiteSpace(txtCities.Text);
            btnRemoveAllCities.IsEnabled = lstCities.Items.Count > 0;
        }

        private void RemoveAllCities_Click(object sender, RoutedEventArgs e)
        {
            lstCities.Items.Clear();
            btnRemoveAllCities.IsEnabled = false;
        }

        private void Parameters_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (radioSingleThreaded.IsChecked == true)
            {
                btnStart.IsEnabled = !string.IsNullOrWhiteSpace(txtPopulationSize.Text) &&
                                 !string.IsNullOrWhiteSpace(txtMutationRate.Text);
            } else
            {
                btnStart.IsEnabled = !string.IsNullOrWhiteSpace(txtThreads.Text) &&
                                 !string.IsNullOrWhiteSpace(txtPopulationSize.Text) &&
                                 !string.IsNullOrWhiteSpace(txtMutationRate.Text);
            }
        }

        private void AddCities_Click(object sender, RoutedEventArgs e)
        {
            CultureInfo cultureInfo = new CultureInfo("en-US");
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
                if (!double.TryParse(parts[1].Replace(",", "."), NumberStyles.Any, cultureInfo, out x) || !double.TryParse(parts[2].Replace(",", "."), NumberStyles.Any, cultureInfo, out y))
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
            int numberOfThreads = 1;
            if (radioSingleThreaded.IsChecked == false)
            {
                if (!int.TryParse(txtThreads.Text, out numberOfThreads))
                {
                    txtResults.Text = "Liczba wątków musi być liczbą całkowitą.";
                    return;
                }
                //if (numberOfThreads > Environment.ProcessorCount)
                //{
                //    txtResults.Text = $"Liczba wątków nie może przekraczać liczby dostępnych procesorów ({Environment.ProcessorCount}).";
                //    return;
                //}
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
            if(lstCities.Items.Count  < 2)
            {
                txtResults.Text = "Dodaj więcej miast, minimalna liczba miast wynosi 2.";
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
            Route bestRoute;
            if (radioSingleThreaded.IsChecked == true)
            {
                bestRoute = await Task.Run(() => ga.RunGeneticAlgorithm(initialPopulation, 1000, 5, mutationRate));
            } else
            {
                bestRoute = await ga.RunGeneticAlgorithmParallel(initialPopulation, 1000, 5, mutationRate, numberOfThreads);
            }

            stopwatch.Stop();
            progressBar.Visibility = Visibility.Hidden;

            // Znalezienie maxX oraz maxY w celu przeskalowania wykresu
            double maxX = bestRoute.Cities.Max(c => c.Location.X);
            double maxY = bestRoute.Cities.Max(c => c.Location.Y);

            // Display the result
            ResultWindow resultWindow = new ResultWindow(bestRoute, maxX, maxY);
            resultWindow.Show();
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
