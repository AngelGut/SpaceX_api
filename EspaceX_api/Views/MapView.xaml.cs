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
    /// Responsabilidad única: dibujar el mapa en el Canvas y capturar
    /// eventos de mouse, delegando toda la lógica al MapViewModel.
    /// (Single Responsibility Principle)
    /// </summary>
    public partial class MapView : UserControl
    {
        // Acceso tipado al ViewModel vinculado al DataContext
        private MapViewModel? ViewModel => DataContext as MapViewModel;

        public MapView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            Loaded += OnLoaded;
        }

        /// <summary>
        /// Se suscribe a PropertyChanged del ViewModel para redibujar
        /// cuando cambian datos relevantes (sitios, zoom, pan).
        /// </summary>
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is MapViewModel vm)
            {
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

        /// <summary>
        /// Enlaza los eventos del mouse al Canvas una vez que el control está cargado.
        /// </summary>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            MapCanvas.MouseWheel += OnMouseWheel;
            MapCanvas.MouseLeftButtonDown += OnMouseLeftButtonDown;
            MapCanvas.MouseLeftButtonUp += OnMouseLeftButtonUp;
            MapCanvas.MouseMove += OnMouseMove;
            MapCanvas.SizeChanged += (s, args) => DrawMap();
        }


        /// <summary>
        /// Limpia el Canvas y redibuja la grilla y todos los sitios de lanzamiento.
        /// </summary>
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

                if (screenX < 0 || screenX > width || screenY < 0 || screenY > height)
                    continue;

                DrawLaunchSite(screenX, screenY, site);
            }
        }

        /// <summary>
        /// Dibuja una grilla de líneas de latitud y longitud sobre el Canvas.
        /// </summary>
        private void DrawGrid(double width, double height)
        {
            var gridColor = new SolidColorBrush(Color.FromRgb(40, 40, 40));

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

        /// <summary>
        /// Dibuja un punto rojo con halo y etiqueta para un sitio de lanzamiento.
        /// Al hacer click actualiza SelectedSite en el ViewModel.
        /// </summary>
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

            dot.MouseLeftButtonDown += (s, e) =>
            {
                if (ViewModel != null)
                    ViewModel.SelectedSite = site;
                e.Handled = true;
            };
            MapCanvas.Children.Add(dot);

            // Etiqueta con nombre del sitio
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


        /// <summary>
        /// Zoom con rueda del mouse — delega al comando del ViewModel.
        /// La View no decide cuánto hacer zoom, solo notifica la dirección.
        /// </summary>
        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (ViewModel == null) return;

            if (e.Delta > 0)
                ViewModel.ZoomInCommand.Execute(null);
            else
                ViewModel.ZoomOutCommand.Execute(null);
        }

        /// <summary>
        /// Inicio del drag — la View solo pasa las coordenadas al ViewModel.
        /// El estado _isDragging vive en el ViewModel, no aquí.
        /// </summary>
        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(MapCanvas);
            ViewModel?.BeginDrag(pos.X, pos.Y);
            MapCanvas.CaptureMouse();
        }

        /// <summary>
        /// Fin del drag — la View notifica al ViewModel que terminó.
        /// </summary>
        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ViewModel?.EndDrag();
            MapCanvas.ReleaseMouseCapture();
        }

        /// <summary>
        /// Movimiento del mouse durante el drag — la View pasa las coordenadas actuales.
        /// El ViewModel calcula el delta y actualiza PanX/PanY.
        /// </summary>
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(MapCanvas);
            ViewModel?.UpdateDrag(pos.X, pos.Y);
        }
    }
}