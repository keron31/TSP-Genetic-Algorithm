using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace TSP_Genetic_Algorithm
{
    public partial class ResultWindow : Window
    {
        private const double GridIntervalPercentage = 0.1;
        private const double MarginPercentage = 0.1;
        private List<City> bestRoute;
        private double maxX;
        private double maxY;

        public ResultWindow(Route bestRouteList, double maxX, double maxY)
        {
            InitializeComponent();

            this.bestRoute = new List<City>();
            foreach (var city in bestRouteList.Cities)
            {
                this.bestRoute.Add(city);
            }

            this.maxX = maxX * (1 + MarginPercentage); // Adding margin to X
            this.maxY = maxY * (1 + MarginPercentage); // Adding margin to Y

            canvas.SizeChanged += Draw;
        }

        private void Draw(object sender, SizeChangedEventArgs e)
        {
            canvas.Children.Clear(); // Clear the canvas before redrawing

            // Dodawanie siatki
            for (double i = GridIntervalPercentage; i < 1; i += GridIntervalPercentage)
            {
                double x = i * (canvas.ActualWidth - 20) + 10;
                double y = i * (canvas.ActualHeight - 20) + 10;

                // Linie pionowe
                Line verticalLine = new Line
                {
                    Stroke = Brushes.Gray,
                    X1 = x,
                    Y1 = 0,
                    X2 = x,
                    Y2 = canvas.ActualHeight - 10
                };
                canvas.Children.Add(verticalLine);

                // Etykiety dla osi X
                TextBlock xLabel = new TextBlock
                {
                    Text = (i * maxX).ToString("0"),
                    Foreground = Brushes.Black
                };
                Canvas.SetLeft(xLabel, x);
                Canvas.SetBottom(xLabel, canvas.ActualHeight - xLabel.ActualHeight - 20);
                canvas.Children.Add(xLabel);

                // Linie poziome
                Line horizontalLine = new Line
                {
                    Stroke = Brushes.Gray,
                    X1 = 0,
                    Y1 = y,
                    X2 = canvas.ActualWidth - 10,
                    Y2 = y
                };
                canvas.Children.Add(horizontalLine);

                // Etykiety dla osi Y
                TextBlock yLabel = new TextBlock
                {
                    Text = ((1 - i) * maxY).ToString("0"),
                    Foreground = Brushes.Black
                };
                Canvas.SetLeft(yLabel, 0);
                Canvas.SetTop(yLabel, y - xLabel.ActualHeight);
                canvas.Children.Add(yLabel);
            }

            // Rysowanie linii
            DrawRouteLines();

            // Rysowanie punktów dla miast
            DrawCityDots();
        }

        private void DrawRouteLines()
        {
            for (int i = 0; i < bestRoute.Count - 1; i++)
            {
                // Rysowanie linii pomiędzy miastami
                Line line = new Line
                {
                    Stroke = Brushes.Black,
                    X1 = bestRoute[i].Location.X / maxX * (canvas.ActualWidth - 20) + 10,
                    Y1 = canvas.ActualHeight - bestRoute[i].Location.Y / maxY * (canvas.ActualHeight - 20) - 10,
                    X2 = bestRoute[i + 1].Location.X / maxX * (canvas.ActualWidth - 20) + 10,
                    Y2 = canvas.ActualHeight - bestRoute[i + 1].Location.Y / maxY * (canvas.ActualHeight - 20) - 10,
                };
                canvas.Children.Add(line);
            }

            // Łączenie ostatniego miasta z pierwszym
            Line finalLine = new Line
            {
                Stroke = Brushes.Black,
                X1 = bestRoute[^1].Location.X / maxX * (canvas.ActualWidth - 20) + 10,
                Y1 = canvas.ActualHeight - bestRoute[^1].Location.Y / maxY * (canvas.ActualHeight - 20) - 10,
                X2 = bestRoute[0].Location.X / maxX * (canvas.ActualWidth - 20) + 10,
                Y2 = canvas.ActualHeight - bestRoute[0].Location.Y / maxY * (canvas.ActualHeight - 20) - 10
            };
            canvas.Children.Add(finalLine);
        }


        private void DrawCityDots()
        {
            for (int i = 0; i < bestRoute.Count; i++)
            {
                var city = bestRoute[i];

                Ellipse dot = new Ellipse
                {
                    Stroke = Brushes.Red,
                    Fill = Brushes.Red,
                    Width = 5,
                    Height = 5
                };

                Canvas.SetLeft(dot, city.Location.X / maxX * (canvas.ActualWidth - 20) + 10 - dot.Width / 2);
                Canvas.SetTop(dot, canvas.ActualHeight - city.Location.Y / maxY * (canvas.ActualHeight - 20) - 10 - dot.Height / 2);
                canvas.Children.Add(dot);

                TextBlock cityLabel = new TextBlock
                {
                    Text = (i + 1).ToString(),  // Kolejność miasta
                    Foreground = Brushes.Blue,
                    FontSize = 12,
                    FontWeight = FontWeights.Bold
                };
                Canvas.SetLeft(cityLabel, city.Location.X / maxX * (canvas.ActualWidth - 20) + 10 + dot.Width);
                Canvas.SetTop(cityLabel, canvas.ActualHeight - city.Location.Y / maxY * (canvas.ActualHeight - 20) - 10 - dot.Height / 2);
                canvas.Children.Add(cityLabel);
            }
        }
    }
}
