using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace EspaceX_api.Views
{
    public partial class HomeView : UserControl
    {
        private readonly Random _rng = new Random();
        private readonly DispatcherTimer _rocketTimer = new DispatcherTimer();
        private readonly List<RocketData> _rockets = new();

        public HomeView()
        {
            InitializeComponent();
        }

        private void HomeView_Loaded(object sender, RoutedEventArgs e)
        {
            GenerateStars();
            ScheduleRockets();
        }


        private void GenerateStars()
        {
            StarsCanvas.Children.Clear();
            double w = ActualWidth > 0 ? ActualWidth : 1000;
            double h = ActualHeight > 0 ? ActualHeight : 700;

            for (int i = 0; i < 220; i++)
            {
                double size = _rng.NextDouble() < 0.15 ? _rng.Next(2, 4) : 1;
                double opacity = _rng.NextDouble() * 0.5 + 0.15;

                var star = new Ellipse
                {
                    Width = size,
                    Height = size,
                    Fill = new SolidColorBrush(Color.FromRgb(220, 230, 255)),
                    Opacity = opacity
                };

                Canvas.SetLeft(star, _rng.NextDouble() * w);
                Canvas.SetTop(star, _rng.NextDouble() * h);
                StarsCanvas.Children.Add(star);

                // Twinkling suave en algunas estrellas
                if (_rng.NextDouble() < 0.3)
                {
                    var twinkle = new DoubleAnimation
                    {
                        From = opacity,
                        To = opacity * 0.2,
                        Duration = TimeSpan.FromSeconds(_rng.NextDouble() * 3 + 1.5),
                        AutoReverse = true,
                        RepeatBehavior = RepeatBehavior.Forever,
                        BeginTime = TimeSpan.FromSeconds(_rng.NextDouble() * 4)
                    };
                    star.BeginAnimation(OpacityProperty, twinkle);
                }
            }
        }


        private void ScheduleRockets()
        {
            // Lanza el primero inmediatamente tras 1.2s
            LaunchRocket(1.2);

            // Timer que lanza cohetes cada 5-9 segundos
            _rocketTimer.Interval = TimeSpan.FromSeconds(6);
            _rocketTimer.Tick += (s, e) =>
            {
                _rocketTimer.Interval = TimeSpan.FromSeconds(_rng.Next(5, 10));
                LaunchRocket(0);
            };
            _rocketTimer.Start();
        }

        private void LaunchRocket(double delaySeconds)
        {
            double w = ActualWidth > 0 ? ActualWidth : 1000;
            double h = ActualHeight > 0 ? ActualHeight : 700;

            // Posicion X aleatoria en el tercio central del ancho
            double startX = _rng.NextDouble() * (w * 0.7) + w * 0.1;
            double tiltAngle = (_rng.NextDouble() - 0.5) * 20; // -10 a +10 grados

            // Tamanio aleatorio (pequeno, mediano, grande)
            double scale = _rng.NextDouble() * 0.8 + 0.5;
            Color exhaustColor = _rng.NextDouble() > 0.5
                ? Color.FromRgb(255, 100, 30)
                : Color.FromRgb(80, 160, 255);

            // Construir el cohete como Canvas
            var rocket = BuildRocketCanvas(scale, exhaustColor);
            Canvas.SetLeft(rocket, startX);
            Canvas.SetTop(rocket, h + 20);
            rocket.Opacity = 0;
            rocket.RenderTransformOrigin = new Point(0.5, 0.5);
            rocket.RenderTransform = new RotateTransform(tiltAngle);
            RocketsCanvas.Children.Add(rocket);

            double duration = _rng.NextDouble() * 4 + 6; // 6-10s

            // Animacion vertical
            var moveAnim = new DoubleAnimation
            {
                From = h + 20,
                To = -80,
                Duration = TimeSpan.FromSeconds(duration),
                BeginTime = TimeSpan.FromSeconds(delaySeconds),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };

            // Fade in al inicio
            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 0.85,
                Duration = TimeSpan.FromSeconds(0.8),
                BeginTime = TimeSpan.FromSeconds(delaySeconds)
            };

            // Fade out al final
            var fadeOut = new DoubleAnimation
            {
                From = 0.85,
                To = 0,
                Duration = TimeSpan.FromSeconds(1.2),
                BeginTime = TimeSpan.FromSeconds(delaySeconds + duration - 1.2)
            };

            fadeOut.Completed += (s, e) =>
            {
                RocketsCanvas.Children.Remove(rocket);
            };

            rocket.BeginAnimation(Canvas.TopProperty, moveAnim);
            rocket.BeginAnimation(OpacityProperty, fadeIn);

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(delaySeconds + duration - 1.2) };
            timer.Tick += (s, e) =>
            {
                timer.Stop();
                rocket.BeginAnimation(OpacityProperty, fadeOut);
            };
            timer.Start();
        }

        private Canvas BuildRocketCanvas(double scale, Color exhaustColor)
        {
            var c = new Canvas
            {
                Width = 20 * scale,
                Height = 60 * scale
            };

            // Cuerpo
            var body = new Rectangle
            {
                Width = 6 * scale,
                Height = 26 * scale,
                Fill = Brushes.White,
                RadiusX = 3 * scale,
                RadiusY = 3 * scale
            };
            Canvas.SetLeft(body, 7 * scale);
            Canvas.SetTop(body, 8 * scale);
            c.Children.Add(body);

            // Punta (triangulo)
            var tip = new Polygon
            {
                Points = new PointCollection {
                    new Point(10 * scale, 0),
                    new Point(7 * scale, 10 * scale),
                    new Point(13 * scale, 10 * scale)
                },
                Fill = Brushes.White
            };
            c.Children.Add(tip);

            // Aletas izquierda
            var finL = new Polygon
            {
                Points = new PointCollection {
                    new Point(7 * scale, 28 * scale),
                    new Point(2 * scale, 38 * scale),
                    new Point(7 * scale, 36 * scale)
                },
                Fill = new SolidColorBrush(Color.FromRgb(200, 200, 200))
            };
            c.Children.Add(finL);

            // Aletas derecha
            var finR = new Polygon
            {
                Points = new PointCollection {
                    new Point(13 * scale, 28 * scale),
                    new Point(18 * scale, 38 * scale),
                    new Point(13 * scale, 36 * scale)
                },
                Fill = new SolidColorBrush(Color.FromRgb(200, 200, 200))
            };
            c.Children.Add(finR);

            // Exhaust glow
            var glow = new Ellipse
            {
                Width = 12 * scale,
                Height = 22 * scale,
                Fill = new RadialGradientBrush(
                    exhaustColor,
                    Color.FromArgb(0, exhaustColor.R, exhaustColor.G, exhaustColor.B))
            };
            Canvas.SetLeft(glow, 4 * scale);
            Canvas.SetTop(glow, 34 * scale);
            c.Children.Add(glow);

            // Animar el exhaust (pulso)
            var pulse = new DoubleAnimation
            {
                From = 0.9,
                To = 0.3,
                Duration = TimeSpan.FromSeconds(0.3),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            glow.BeginAnimation(OpacityProperty, pulse);

            return c;
        }

        private class RocketData
        {
            public Canvas Visual { get; set; } = null!;
        }
    }
}