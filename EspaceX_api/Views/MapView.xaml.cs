using EspaceX_api.Models;
using EspaceX_api.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace EspaceX_api.Views
{
    /// <summary>
    /// Code-behind de MapView.
    /// Responsabilidad unica: dibujar el mapa en el Canvas y capturar
    /// eventos de mouse, delegando toda la logica al MapViewModel.
    /// (Single Responsibility Principle)
    /// </summary>
    public partial class MapView : UserControl
    {
        private MapViewModel? ViewModel => DataContext as MapViewModel;

        // Colores del mapa
        private static readonly SolidColorBrush ContinentFill = new(Color.FromRgb(22, 48, 90));
        private static readonly SolidColorBrush ContinentStroke = new(Color.FromRgb(40, 80, 140));
        private static readonly SolidColorBrush GridColor = new(Color.FromRgb(30, 58, 92));
        private static readonly SolidColorBrush OceanColor = new(Color.FromRgb(6, 12, 20));

        public MapView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            Loaded += OnLoaded;
        }

        /// <summary>
        /// Se suscribe a PropertyChanged del ViewModel para redibujar
        /// cuando cambian datos relevantes.
        /// Incluye SelectedSite para dibujar el punto al seleccionar un sitio.
        /// </summary>
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is MapViewModel vm)
            {
                vm.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName is nameof(vm.LaunchSites)
                                          or nameof(vm.SelectedSite)
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
        /// Enlaza los eventos del mouse al Canvas una vez que el control esta cargado.
        /// Dibuja el mapa base (oceano + continentes + grilla) al arrancar.
        /// </summary>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            MapCanvas.MouseWheel += OnMouseWheel;
            MapCanvas.MouseLeftButtonDown += OnMouseLeftButtonDown;
            MapCanvas.MouseLeftButtonUp += OnMouseLeftButtonUp;
            MapCanvas.MouseMove += OnMouseMove;
            MapCanvas.SizeChanged += (s, args) => DrawMap();

            // Dibuja el mapa base apenas carga la vista
            DrawMap();
        }

        /// <summary>
        /// Punto de entrada principal: limpia y redibuja todo el canvas.
        /// Orden: oceano > continentes > grilla > sitio seleccionado.
        /// Los puntos rojos solo aparecen cuando el usuario selecciona
        /// un sitio en la lista derecha. (No se dibujan todos al cargar)
        /// </summary>
        private void DrawMap()
        {
            MapCanvas.Children.Clear();

            double width = MapCanvas.ActualWidth;
            double height = MapCanvas.ActualHeight;

            if (width <= 0 || height <= 0) return;

            // 1. Fondo oceano
            MapCanvas.Children.Add(new Rectangle
            {
                Width = width,
                Height = height,
                Fill = OceanColor
            });

            // 2. Continentes — siempre visibles
            DrawContinents(width, height);

            // 3. Grilla de latitud y longitud
            DrawGrid(width, height);

            // 4. Solo dibuja el sitio seleccionado en la lista derecha.
            // No se dibuja nada hasta que el usuario haga clic en un sitio.
            if (ViewModel == null || ViewModel.SelectedSite == null) return;

            var selected = ViewModel.SelectedSite;
            var (sx, sy) = ViewModel.GeographicToScreenCoordinates(
                selected.Latitude, selected.Longitude, width, height);

            if (sx >= 0 && sx <= width && sy >= 0 && sy <= height)
                DrawLaunchSite(sx, sy, selected);
        }

        /// <summary>
        /// Convierte coordenadas geograficas a pixeles del canvas.
        /// Usa el ViewModel si esta disponible para aplicar zoom y pan.
        /// </summary>
        private (double x, double y) GeoToCanvas(double lat, double lon, double w, double h)
        {
            if (ViewModel != null)
                return ViewModel.GeographicToScreenCoordinates(lat, lon, w, h);

            // Fallback sin ViewModel: proyeccion Mercator simple
            double x = (lon + 180.0) / 360.0;
            double latRad = lat * Math.PI / 180.0;
            double mercN = Math.Log(Math.Tan(Math.PI / 4 + latRad / 2));
            double y = 0.5 - mercN / (2 * Math.PI);
            return (x * w, y * h);
        }

        /// <summary>
        /// Dibuja los continentes como poligonos usando coordenadas geograficas reales.
        /// Estilo: azul oscuro minimalista con borde visible.
        /// </summary>
        private void DrawContinents(double width, double height)
        {
            var continents = GetContinentCoordinates();

            foreach (var continent in continents)
            {
                var points = new PointCollection();
                foreach (var (lat, lon) in continent)
                {
                    var (px, py) = GeoToCanvas(lat, lon, width, height);
                    points.Add(new Point(px, py));
                }

                MapCanvas.Children.Add(new Polygon
                {
                    Points = points,
                    Fill = ContinentFill,
                    Stroke = ContinentStroke,
                    StrokeThickness = 0.8
                });
            }
        }

        /// <summary>
        /// Coordenadas geograficas (lat, lon) simplificadas de cada continente.
        /// </summary>
        private static (double lat, double lon)[][] GetContinentCoordinates()
        {
            return new[]
            {
                // America del Norte
                new (double lat, double lon)[]
                {
                    (70,-140),(72,-120),(70,-100),(68,-85),(60,-75),
                    (50,-55),(47,-53),(45,-60),(44,-66),(42,-70),
                    (40,-74),(35,-76),(30,-81),(25,-80),(20,-87),
                    (15,-85),(10,-83),(8,-77),(9,-79),(8,-77),
                    (10,-85),(15,-92),(20,-105),(22,-110),(25,-110),
                    (30,-116),(32,-117),(38,-123),(48,-124),(54,-130),
                    (58,-137),(60,-145),(63,-162),(65,-168),(68,-166),
                    (70,-157),(71,-148),(70,-140)
                },

                // America del Sur
                new (double lat, double lon)[]
                {
                    (10,-75),(8,-77),(5,-77),(1,-80),
                    (-5,-81),(-10,-78),(-15,-76),(-18,-70),
                    (-22,-68),(-25,-70),(-30,-71),(-35,-72),
                    (-40,-73),(-45,-75),(-50,-75),(-52,-70),
                    (-54,-65),(-55,-68),(-52,-60),(-48,-65),
                    (-45,-65),(-40,-62),(-35,-57),(-30,-50),
                    (-25,-48),(-20,-40),(-15,-39),(-10,-37),
                    (-5,-35),(0,-50),(5,-52),(8,-60),
                    (10,-63),(10,-75)
                },

                // Europa
                new (double lat, double lon)[]
                {
                    (71,28),(70,20),(69,18),(65,14),(58,5),
                    (51,2),(48,-5),(44,-8),(36,-9),(36,-6),
                    (37,-4),(38,0),(40,3),(43,3),(44,8),
                    (44,12),(41,12),(38,15),(38,16),(40,18),
                    (41,19),(42,20),(44,22),(44,28),(46,30),
                    (48,24),(50,22),(54,20),(56,22),(59,25),
                    (60,30),(65,28),(68,28),(71,28)
                },

                // Africa
                new (double lat, double lon)[]
                {
                    (37,-5),(37,10),(37,12),(33,12),(30,32),
                    (27,34),(22,37),(15,42),(12,44),(10,42),
                    (2,42),(-2,42),(-5,40),(-10,40),(-15,35),
                    (-20,35),(-25,33),(-30,30),(-34,26),(-35,20),
                    (-30,17),(-25,15),(-18,12),(-10,14),(-5,10),
                    (0,9),(5,2),(4,7),(2,3),(4,-2),
                    (5,-5),(5,-8),(4,-11),(6,-14),(10,-15),
                    (15,-17),(20,-17),(25,-15),(30,-10),(35,-6),
                    (37,-5)
                },

                // Asia
                new (double lat, double lon)[]
                {
                    (70,30),(72,60),(73,80),(72,100),(70,130),
                    (68,140),(64,142),(60,140),(55,135),(50,140),
                    (45,135),(40,130),(35,130),(30,121),(25,120),
                    (20,110),(10,104),(5,103),(1,104),(1,108),
                    (5,115),(5,108),(10,100),(15,98),(20,93),
                    (22,88),(20,85),(15,80),(10,79),(8,77),
                    (22,60),(25,57),(24,51),(30,48),(37,36),
                    (37,27),(41,29),(42,28),(44,34),(44,40),
                    (48,44),(50,50),(55,60),(55,70),(60,60),
                    (65,50),(68,35),(70,30)
                },

                // Australia
                new (double lat, double lon)[]
                {
                    (-15,130),(-13,136),(-12,136),(-13,141),
                    (-15,145),(-18,147),(-22,150),(-25,153),
                    (-28,153),(-32,152),(-34,151),(-38,147),
                    (-39,146),(-38,140),(-35,137),(-32,134),
                    (-32,129),(-33,122),(-32,115),(-30,115),
                    (-25,114),(-22,114),(-18,122),(-16,123),
                    (-15,128),(-15,130)
                },

                // Groenlandia
                new (double lat, double lon)[]
                {
                    (83,-35),(83,-15),(80,-15),(76,-18),
                    (72,-22),(68,-27),(65,-37),(63,-42),
                    (65,-52),(68,-53),(72,-55),(76,-68),
                    (78,-72),(80,-68),(83,-45),(83,-35)
                },
            };
        }

        /// <summary>
        /// Dibuja la grilla de latitud y longitud visible sobre el mapa.
        /// </summary>
        private void DrawGrid(double width, double height)
        {
            for (int lat = -60; lat <= 80; lat += 30)
            {
                var (_, y) = GeoToCanvas(lat, 0, width, height);
                MapCanvas.Children.Add(new Line
                {
                    X1 = 0,
                    Y1 = y,
                    X2 = width,
                    Y2 = y,
                    Stroke = GridColor,
                    StrokeThickness = 0.6
                });
            }

            for (int lon = -180; lon <= 180; lon += 30)
            {
                var (x, _) = GeoToCanvas(0, lon, width, height);
                MapCanvas.Children.Add(new Line
                {
                    X1 = x,
                    Y1 = 0,
                    X2 = x,
                    Y2 = height,
                    Stroke = GridColor,
                    StrokeThickness = 0.6
                });
            }
        }

        /// <summary>
        /// Dibuja un punto rojo con halo y etiqueta para el sitio seleccionado.
        /// Al hacer click en el punto actualiza SelectedSite en el ViewModel.
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

            // Punto rojo principal con borde blanco
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
                Foreground = new SolidColorBrush(Color.FromRgb(180, 220, 255)),
                FontSize = 10
            };
            Canvas.SetLeft(label, x + radius + 3);
            Canvas.SetTop(label, y - 7);
            MapCanvas.Children.Add(label);
        }

        // ==================== EVENTOS DE MOUSE ====================

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (ViewModel == null) return;
            if (e.Delta > 0) ViewModel.ZoomInCommand.Execute(null);
            else ViewModel.ZoomOutCommand.Execute(null);
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(MapCanvas);
            ViewModel?.BeginDrag(pos.X, pos.Y);
            MapCanvas.CaptureMouse();
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ViewModel?.EndDrag();
            MapCanvas.ReleaseMouseCapture();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(MapCanvas);
            ViewModel?.UpdateDrag(pos.X, pos.Y);
        }
    }
}