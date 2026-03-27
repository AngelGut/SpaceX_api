using EspaceX_api.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace EspaceX_api.Views
{
    public partial class HomeView : UserControl
    {
        private readonly Random _random = new Random();

        public HomeView(HomeViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += (s, e) => GenerateStars();
        }

        public HomeView()
        {
            InitializeComponent();
            Loaded += (s, e) => GenerateStars();
        }

        private void GenerateStars()
        {
            for (int i = 0; i < 100; i++)
            {
                double size = _random.NextDouble() * 2 + 0.5;
                var star = new Ellipse
                {
                    Width = size,
                    Height = size,
                    Fill = Brushes.White,
                    Opacity = 0.1
                };

                Canvas.SetLeft(star, _random.NextDouble() * StarsCanvas.ActualWidth);
                Canvas.SetTop(star, _random.NextDouble() * StarsCanvas.ActualHeight);

                var twinkle = new DoubleAnimation
                {
                    From = 0.05,
                    To = 0.85,
                    Duration = TimeSpan.FromSeconds(2 + _random.NextDouble() * 4),
                    AutoReverse = true,
                    RepeatBehavior = RepeatBehavior.Forever,
                    BeginTime = TimeSpan.FromSeconds(_random.NextDouble() * 6)
                };
                star.BeginAnimation(UIElement.OpacityProperty, twinkle);
                StarsCanvas.Children.Add(star);
            }

            for (int i = 0; i < 3; i++)
                SpawnShootingStar(TimeSpan.FromSeconds(_random.NextDouble() * 5));
        }

        private void SpawnShootingStar(TimeSpan delay)
        {
            double startX = _random.NextDouble() * StarsCanvas.ActualWidth * 0.6;
            double startY = _random.NextDouble() * StarsCanvas.ActualHeight * 0.4;
            double length = 60 + _random.NextDouble() * 80;
            double distance = StarsCanvas.ActualWidth * 0.6;
            double duration = 1.5 + _random.NextDouble() * 2;

            var line = new Line
            {
                X1 = startX,
                Y1 = startY,
                X2 = startX - length,
                Y2 = startY - length * 0.4,
                StrokeThickness = 1.5,
                Stroke = new LinearGradientBrush(
                    Color.FromArgb(0, 0, 212, 255),
                    Color.FromArgb(200, 200, 240, 255),
                    new Point(0, 0), new Point(1, 0)),
                Opacity = 0
            };

            StarsCanvas.Children.Add(line);

            var moveX1 = new DoubleAnimation { From = startX, To = startX + distance, Duration = TimeSpan.FromSeconds(duration), BeginTime = delay };
            var moveX2 = new DoubleAnimation { From = startX - length, To = startX - length + distance, Duration = TimeSpan.FromSeconds(duration), BeginTime = delay };
            var moveY1 = new DoubleAnimation { From = startY, To = startY + distance * 0.5, Duration = TimeSpan.FromSeconds(duration), BeginTime = delay };
            var moveY2 = new DoubleAnimation { From = startY - length * 0.4, To = startY - length * 0.4 + distance * 0.5, Duration = TimeSpan.FromSeconds(duration), BeginTime = delay };
            var fadeIn = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromSeconds(0.3), BeginTime = delay };
            var fadeOut = new DoubleAnimation { From = 1, To = 0, Duration = TimeSpan.FromSeconds(0.3), BeginTime = delay + TimeSpan.FromSeconds(duration - 0.3) };

            fadeOut.Completed += (s, e) =>
            {
                StarsCanvas.Children.Remove(line);
                SpawnShootingStar(TimeSpan.FromSeconds(_random.NextDouble() * 4 + 2));
            };

            line.BeginAnimation(Line.X1Property, moveX1);
            line.BeginAnimation(Line.X2Property, moveX2);
            line.BeginAnimation(Line.Y1Property, moveY1);
            line.BeginAnimation(Line.Y2Property, moveY2);
            line.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            line.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }
    }
}