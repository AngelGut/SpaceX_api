using EspaceX_api.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace EspaceX_api.Views
{
    /// <summary>
    /// Vista principal del Home.
    /// Responsabilidad: presentación visual + animaciones del fondo espacial.
    /// El DataContext es HomeViewModel (inyectado por constructor o DataTemplate).
    /// </summary>
    public partial class HomeView : UserControl
    {
        // Generador de números aleatorios para posiciones y tiempos de estrellas
        private readonly Random _random = new Random();

        /// <summary>
        /// Constructor principal usado por el contenedor DI.
        /// Recibe el ViewModel ya instanciado y lo asigna como DataContext.
        /// </summary>
        public HomeView(HomeViewModel viewModel)
        {
            InitializeComponent();
            // Asignar ViewModel al DataContext para que los bindings funcionen
            DataContext = viewModel;
            // Generar estrellas cuando la vista ya tiene dimensiones reales
            Loaded += (s, e) => GenerateStars();
        }

        /// <summary>
        /// Constructor sin parámetros requerido por WPF DataTemplate.
        /// El DataContext lo asigna automáticamente el ContentControl
        /// cuando detecta el tipo del ViewModel en el DataTemplate.
        /// </summary>
        public HomeView()
        {
            InitializeComponent();
            Loaded += (s, e) => GenerateStars();
        }

        /// <summary>
        /// Genera 100 estrellas con parpadeo aleatorio sobre el StarsCanvas,
        /// y lanza 3 estrellas fugaces con delays escalonados.
        /// Se ejecuta en el evento Loaded para tener las dimensiones reales del canvas.
        /// </summary>
        private void GenerateStars()
        {
            for (int i = 0; i < 100; i++)
            {
                // Tamaño aleatorio entre 0.5px y 2.5px para simular profundidad
                double size = _random.NextDouble() * 2 + 0.5;

                var star = new Ellipse
                {
                    Width = size,
                    Height = size,
                    Fill = Brushes.White,
                    Opacity = 0.1 // Inicia casi invisible
                };

                // Posición aleatoria dentro del canvas
                Canvas.SetLeft(star, _random.NextDouble() * StarsCanvas.ActualWidth);
                Canvas.SetTop(star, _random.NextDouble() * StarsCanvas.ActualHeight);

                // Animación de parpadeo: oscila entre opacidad mínima y máxima
                var twinkle = new DoubleAnimation
                {
                    From = 0.05,
                    To = 0.85,
                    Duration = TimeSpan.FromSeconds(2 + _random.NextDouble() * 4),
                    AutoReverse = true,                    // Regresa al valor inicial
                    RepeatBehavior = RepeatBehavior.Forever,
                    BeginTime = TimeSpan.FromSeconds(_random.NextDouble() * 6) // Desfase para que no parpadeen todas igual
                };

                star.BeginAnimation(UIElement.OpacityProperty, twinkle);
                StarsCanvas.Children.Add(star);
            }

            // Lanzar 3 estrellas fugaces con delays distintos al inicio
            for (int i = 0; i < 3; i++)
                SpawnShootingStar(TimeSpan.FromSeconds(_random.NextDouble() * 5));
        }

        /// <summary>
        /// Crea y anima una estrella fugaz diagonal sobre el canvas.
        /// Al terminar se auto-destruye y crea otra con delay aleatorio,
        /// generando un loop infinito de estrellas fugaces.
        /// </summary>
        /// <param name="delay">Tiempo de espera antes de que aparezca la estrella</param>
        private void SpawnShootingStar(TimeSpan delay)
        {
            // Punto de inicio en el tercio superior-izquierdo de la pantalla
            double startX = _random.NextDouble() * StarsCanvas.ActualWidth * 0.6;
            double startY = _random.NextDouble() * StarsCanvas.ActualHeight * 0.4;
            double length = 60 + _random.NextDouble() * 80;   // Longitud de la cola
            double distance = StarsCanvas.ActualWidth * 0.6;   // Distancia que recorre
            double duration = 1.5 + _random.NextDouble() * 2; // Duración del recorrido

            // Línea que representa la cola de la estrella fugaz
            var line = new Line
            {
                X1 = startX,
                Y1 = startY,
                X2 = startX - length,
                Y2 = startY - length * 0.4,
                StrokeThickness = 1.5,
                // Gradiente de transparente a blanco-cian para simular la cola
                Stroke = new LinearGradientBrush(
                    Color.FromArgb(0, 0, 212, 255),       // Inicio: transparente
                    Color.FromArgb(200, 200, 240, 255),   // Fin: blanco-cian
                    new Point(0, 0),
                    new Point(1, 0)),
                Opacity = 0 // Inicia invisible
            };

            StarsCanvas.Children.Add(line);

            // Animaciones de movimiento diagonal (abajo-derecha)
            var moveX1 = new DoubleAnimation { From = startX, To = startX + distance, Duration = TimeSpan.FromSeconds(duration), BeginTime = delay };
            var moveX2 = new DoubleAnimation { From = startX - length, To = startX - length + distance, Duration = TimeSpan.FromSeconds(duration), BeginTime = delay };
            var moveY1 = new DoubleAnimation { From = startY, To = startY + distance * 0.5, Duration = TimeSpan.FromSeconds(duration), BeginTime = delay };
            var moveY2 = new DoubleAnimation { From = startY - length * 0.4, To = startY - length * 0.4 + distance * 0.5, Duration = TimeSpan.FromSeconds(duration), BeginTime = delay };

            // Fade in al inicio del recorrido
            var fadeIn = new DoubleAnimation { From = 0, To = 1, Duration = TimeSpan.FromSeconds(0.3), BeginTime = delay };

            // Fade out al final del recorrido
            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.3),
                BeginTime = delay + TimeSpan.FromSeconds(duration - 0.3)
            };

            // Al terminar el fade out: eliminar línea del canvas y crear una nueva estrella
            fadeOut.Completed += (s, e) =>
            {
                StarsCanvas.Children.Remove(line);
                // Delay aleatorio antes de la próxima estrella fugaz
                SpawnShootingStar(TimeSpan.FromSeconds(_random.NextDouble() * 4 + 2));
            };

            // Aplicar todas las animaciones a la línea
            line.BeginAnimation(Line.X1Property, moveX1);
            line.BeginAnimation(Line.X2Property, moveX2);
            line.BeginAnimation(Line.Y1Property, moveY1);
            line.BeginAnimation(Line.Y2Property, moveY2);
            line.BeginAnimation(UIElement.OpacityProperty, fadeIn);
            line.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }
    }
}