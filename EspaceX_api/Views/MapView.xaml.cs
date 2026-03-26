using EspaceX_api.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace EspaceX_api
{
    public partial class MapView : UserControl
    {
        private MapViewModel _mapVM;
        private Point _lastMousePosition;

        public MapView()
        {
            InitializeComponent();
            this.Loaded += MapView_Loaded;
        }

        private void MapView_Loaded(object sender, RoutedEventArgs e)
        {
            _mapVM = this.DataContext as MapViewModel;
            if (_mapVM != null)
            {
                MapCanvas.MouseWheel += MapCanvas_MouseWheel;
                MapCanvas.MouseLeftButtonDown += MapCanvas_MouseLeftButtonDown;
                MapCanvas.MouseMove += MapCanvas_MouseMove;
                MapCanvas.SizeChanged += MapCanvas_SizeChanged;
                _mapVM.PropertyChanged += MapVM_PropertyChanged;
            }
        }

        private void MapVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName is "LaunchSites" or "ZoomLevel" or "PanX" or "PanY")
            {
                DrawMap();
            }
        }

        private void DrawMap()
        {
            MapCanvas.Children.Clear();

            if (_mapVM?.LaunchSites == null || _mapVM.LaunchSites.Count == 0)
                return;

            double width = MapCanvas.ActualWidth;
            double height = MapCanvas.ActualHeight;

            DrawGridLines(width, height);

            foreach (var site in _mapVM.LaunchSites)
            {
                var (screenX, screenY) = _mapVM.GeographicToScreenCoordinates(
                    site.Latitude, site.Longitude, width, height
                );

                var circle = new Ellipse
                {
                    Width = 12,
                    Height = 12,
                    Fill = new SolidColorBrush(Colors.Red),
                    Stroke = new SolidColorBrush(Colors.DarkRed),
                    StrokeThickness = 2,
                    ToolTip = new ToolTip { Content = site.Info }
                };

                Canvas.SetLeft(circle, screenX - 6);
                Canvas.SetTop(circle, screenY - 6);
                MapCanvas.Children.Add(circle);

                circle.MouseLeftButtonDown += (s, e) =>
                {
                    _mapVM.SelectedSite = site;
                    e.Handled = true;
                };

                circle.MouseEnter += (s, e) =>
                {
                    circle.Fill = new SolidColorBrush(Color.FromRgb(255, 100, 100));
                };

                circle.MouseLeave += (s, e) =>
                {
                    circle.Fill = new SolidColorBrush(Colors.Red);
                };

                var textBlock = new TextBlock
                {
                    Text = site.Name,
                    Foreground = new SolidColorBrush(Colors.White),
                    FontSize = 10,
                    Background = new SolidColorBrush(Color.FromArgb(200, 0, 0, 0))
                };

                Canvas.SetLeft(textBlock, screenX + 15);
                Canvas.SetTop(textBlock, screenY - 6);
                MapCanvas.Children.Add(textBlock);
            }
        }

        private void DrawGridLines(double width, double height)
        {
            var gridPen = new Pen(new SolidColorBrush(Color.FromArgb(50, 100, 100, 100)), 1);

            for (int lon = -180; lon <= 180; lon += 30)
            {
                var (screenX, _) = _mapVM.GeographicToScreenCoordinates(0, lon, width, height);
                var line = new Line
                {
                    X1 = screenX,
                    Y1 = 0,
                    X2 = screenX,
                    Y2 = height,
                    Stroke = gridPen.Brush,
                    StrokeThickness = 0.5,
                    Opacity = 0.3
                };
                MapCanvas.Children.Add(line);
            }

            for (int lat = -60; lat <= 85; lat += 30)
            {
                var (_, screenY) = _mapVM.GeographicToScreenCoordinates(lat, 0, width, height);
                var line = new Line
                {
                    X1 = 0,
                    Y1 = screenY,
                    X2 = width,
                    Y2 = screenY,
                    Stroke = gridPen.Brush,
                    StrokeThickness = 0.5,
                    Opacity = 0.3
                };
                MapCanvas.Children.Add(line);
            }
        }

        private void MapCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
                _mapVM.ZoomInCommand.Execute(null);
            else
                _mapVM.ZoomOutCommand.Execute(null);

            e.Handled = true;
        }

        private void MapCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _lastMousePosition = e.GetPosition(MapCanvas);
            MapCanvas.CaptureMouse();
        }

        private void MapCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPosition = e.GetPosition(MapCanvas);
                double deltaX = currentPosition.X - _lastMousePosition.X;
                double deltaY = currentPosition.Y - _lastMousePosition.Y;

                _mapVM.PanX += deltaX;
                _mapVM.PanY += deltaY;
                _lastMousePosition = currentPosition;

                DrawMap();
            }
        }

        private void MapCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawMap();
        }
    }
}
