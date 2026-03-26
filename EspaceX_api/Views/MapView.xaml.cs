using EspaceX_api.Models;
using EspaceX_api.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace EspaceX_api.Views
{
    /// <summary>
    /// Code-behind de MapView.
    /// Responsabilidad única: dibujar el mapa en el Canvas y manejar
    /// interacciones de mouse (zoom, pan, click en sitios).
    /// Toda la lógica de datos vive en MapViewModel.
    /// (Single Responsibility Principle)
    /// </summary>
    public partial class MapView : UserControl
    {
        // Acceso tipado al ViewModel vinculado al DataContext
        private MapViewModel? ViewModel => DataContext as MapViewModel;

        private bool _isDragging = false;
        private Point _lastMousePosition;

        public MapView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            Loaded += OnLoaded;
        }

        //TODO: Suscripción a cambios en el DataContext para redibujar el mapa cuando cambian datos relevantes
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is MapViewModel vm)
            {
                // Redibujar el mapa cuando cambian datos relevantes del ViewModel
                vm.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName is nameof(vm.LaunchSites)
                                          or nameof(vm.ZoomLevel)
                                          or nameof(vm.PanX)
                                          or nameof(vm.PanY))
                    {
                        DrawMap();
                    }
                };
            }
        }

        //TODO: Suscripción a eventos de mouse para zoom, pan y click en sitios
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            MapCanvas.MouseWheel += OnMouseWheel;
            MapCanvas.MouseLeftButtonDown += OnMouseLeftButtonDown;
            MapCanvas.MouseLeftButtonUp += OnMouseLeftButtonUp;
            MapCanvas.MouseMove += OnMouseMove;
            MapCanvas.SizeChanged += (s, args) => DrawMap();
        }

        //TODO: Método para dibujar el mapa, sitios de lanzamiento y grid en el Canvas
        private void DrawMap()
        {
            MapCanvas.Children.Clear();

            if (ViewModel == null || ViewModel.LaunchSites.Count == 0)
                return;

            double width = MapCanvas.ActualWidth;
            double height = MapCanvas.ActualHeight;

            if (width <= 0 || height <= 0) return;

            DrawGrid(width, height);

            foreach (var site in ViewModel.LaunchSites)
            {
                var (screenX, screenY) = ViewModel.GeographicToScreenCoordinates(
                    site.Latitude, site.Longitude, width, height);

                // Omitir puntos fuera del canvas visible
                if (screenX < 0 || screenX > width || screenY < 0 || screenY > height)
                    continue;

                DrawLaunchSite(screenX, screenY, site);
            }
        }

        private void DrawGrid(double width, double height)
        {
            var gridColor = new SolidColorBrush(Color.FromRgb(40, 40, 40));

            // Líneas horizontales (latitudes)
            for (int lat = -60; lat <= 80; lat += 30)
            {
                var (_, y) = ViewModel!.GeographicToScreenCoordinates(lat, 0, width, height);
                MapCanvas.Children.Add(new Line
                {
                    X1 = 0,
                    Y1 = y,
                    X2 = width,
                    Y2 = y,
                    Stroke = gridColor,
                    StrokeThickness = 0.5
                });
            }

            // Líneas verticales (longitudes)
            for (int lon = -180; lon <= 180; lon += 30)
            {
                var (x, _) = ViewModel!.GeographicToScreenCoordinates(0, lon, width, height);
                MapCanvas.Children.Add(new Line
                {
                    X1 = x,
                    Y1 = 0,
                    X2 = x,
                    Y2 = height,
                    Stroke = gridColor,
                    StrokeThickness = 0.5
                });
            }
        }

        private void DrawLaunchSite(double x, double y, MapPointModel site)
        {
            const double radius = 8;

            // Halo rojo semitransparente
            var glow = new Ellipse
            {
                Width = radius * 2.5,
                Height = radius * 2.5,
                Fill = new SolidColorBrush(Color.FromArgb(60, 255, 50, 50))
            };
            Canvas.SetLeft(glow, x - radius * 1.25);
            Canvas.SetTop(glow, y - radius * 1.25);
            MapCanvas.Children.Add(glow);

            // Punto rojo principal
            var dot = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Fill = new SolidColorBrush(Color.FromRgb(220, 50, 50)),
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 1,
                ToolTip = site.Info,
                Cursor = Cursors.Hand
            };
            Canvas.SetLeft(dot, x - radius);
            Canvas.SetTop(dot, y - radius);

            // Al hacer click en el punto ? actualizar SelectedSite en el ViewModel
            dot.MouseLeftButtonDown += (s, e) =>
            {
                if (ViewModel != null)
                    ViewModel.SelectedSite = site;
                e.Handled = true;
            };
            MapCanvas.Children.Add(dot);

            // Etiqueta con el nombre del sitio
            var label = new TextBlock
            {
                Text = site.Name,
                Foreground = new SolidColorBrush(Colors.White),
                FontSize = 10
            };
            Canvas.SetLeft(label, x + radius + 2);
            Canvas.SetTop(label, y - 7);
            MapCanvas.Children.Add(label);
        }

        ///TODO: Métodos para manejar zoom (mouse wheel) y pan (drag con mouse)
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (ViewModel == null) return;
            if (e.Delta > 0)
                ViewModel.ZoomInCommand.Execute(null);
            else
                ViewModel.ZoomOutCommand.Execute(null);
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _lastMousePosition = e.GetPosition(MapCanvas);
            MapCanvas.CaptureMouse();
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            MapCanvas.ReleaseMouseCapture();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging || ViewModel == null) return;

            var currentPos = e.GetPosition(MapCanvas);
            ViewModel.PanX += currentPos.X - _lastMousePosition.X;
            ViewModel.PanY += currentPos.Y - _lastMousePosition.Y;
            _lastMousePosition = currentPos;
        }
    }
}