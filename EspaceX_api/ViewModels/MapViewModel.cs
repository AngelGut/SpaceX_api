using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EspaceX_api.Models;
using EspaceX_api.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace EspaceX_api.ViewModels
{
    public partial class MapViewModel : ObservableObject
    {
        private readonly ISpaceXApiService _apiService;

        // Accion para volver al Home.
        // Se asigna desde MainViewModel via SetNavigateToHome()
        // porque DI construye este VM antes que MainViewModel.
        private Action _navigateToHome;

        private const double ZoomStep = 0.2;
        private const double ZoomMin = 0.5;
        private const double ZoomMax = 5.0;

        private bool _isDragging = false;
        private double _lastDragX = 0;
        private double _lastDragY = 0;

        [ObservableProperty] private ObservableCollection<MapPointModel> launchSites = new();
        [ObservableProperty] private MapPointModel? selectedSite;
        [ObservableProperty] private ObservableCollection<LaunchModel> selectedSiteLaunches = new();
        [ObservableProperty] private string errorMessage = string.Empty;
        [ObservableProperty] private bool isLoading;
        [ObservableProperty] private double zoomLevel = 1.0;
        [ObservableProperty] private double panX = 0;
        [ObservableProperty] private double panY = 0;

        public MapViewModel(ISpaceXApiService apiService)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        }

        // Llamado desde MainViewModel despues de que DI construye este VM
        public void SetNavigateToHome(Action action) => _navigateToHome = action;

        // Comando que ejecuta el boton "Volver" en MapView.xaml
        [RelayCommand]
        public void GoHome() => _navigateToHome?.Invoke();

        // Se dispara automaticamente cuando el usuario selecciona un sitio en la lista
        partial void OnSelectedSiteChanged(MapPointModel? value)
        {
            SelectedSiteLaunches.Clear();
            if (value == null) return;
            _ = LoadLaunchesForSiteAsync(value.Id);
        }

        // Carga todos los launchpads desde la API y los convierte en puntos del mapa
        [RelayCommand]
        private async Task LoadLaunchSites()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                LaunchSites.Clear();

                var launches = await _apiService.GetLaunchesAsync();

                var launchpadIds = launches
                    .Where(l => !string.IsNullOrEmpty(l.LaunchpadId))
                    .Select(l => l.LaunchpadId)
                    .Distinct()
                    .ToList();

                foreach (var padId in launchpadIds)
                {
                    try
                    {
                        var pad = await _apiService.GetLaunchpadAsync(padId);
                        if (pad == null) continue;

                        LaunchSites.Add(new MapPointModel
                        {
                            Id = pad.Id,
                            Name = pad.Name,
                            Info = $"{pad.Region} — {pad.LaunchSuccesses}/{pad.LaunchAttempts} lanzamientos",
                            Latitude = pad.Latitude,
                            Longitude = pad.Longitude,
                            TotalLaunches = pad.LaunchAttempts
                        });
                    }
                    catch { /* Si falla un pad individual, continuamos con los demas */ }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar sitios: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Zoom y reset — delegan al ViewModel, la View solo llama el comando
        [RelayCommand] private void ZoomIn() => ZoomLevel = Math.Min(ZoomLevel + ZoomStep, ZoomMax);
        [RelayCommand] private void ZoomOut() => ZoomLevel = Math.Max(ZoomLevel - ZoomStep, ZoomMin);
        [RelayCommand] private void ResetView() { ZoomLevel = 1.0; PanX = 0; PanY = 0; }

        // Estos tres metodos son llamados desde MapView.xaml.cs (code-behind)
        // para los eventos de mouse. El estado del drag vive aqui, no en la View. (SRP)
        public void BeginDrag(double x, double y)
        {
            _isDragging = true;
            _lastDragX = x;
            _lastDragY = y;
        }

        public void EndDrag() => _isDragging = false;

        public void UpdateDrag(double currentX, double currentY)
        {
            if (!_isDragging) return;
            PanX += currentX - _lastDragX;
            PanY += currentY - _lastDragY;
            _lastDragX = currentX;
            _lastDragY = currentY;
        }

        // Convierte lat/lon geograficos a coordenadas de pantalla
        // aplicando proyeccion Mercator + zoom + pan.
        // Es public porque MapView.xaml.cs lo llama al dibujar los puntos en el Canvas.
        public (double x, double y) GeographicToScreenCoordinates(
            double latitude, double longitude, double canvasWidth, double canvasHeight)
        {
            double x = (longitude + 180.0) / 360.0;
            double latRad = latitude * Math.PI / 180.0;
            double mercN = Math.Log(Math.Tan(Math.PI / 4 + latRad / 2));
            double y = 0.5 - mercN / (2 * Math.PI);

            return (x * canvasWidth * ZoomLevel + PanX,
                    y * canvasHeight * ZoomLevel + PanY);
        }

        // Filtra los lanzamientos del sitio seleccionado.
        // Se llama automaticamente desde OnSelectedSiteChanged.
        private async Task LoadLaunchesForSiteAsync(string siteId)
        {
            try
            {
                var launches = await _apiService.GetLaunchesAsync();
                SelectedSiteLaunches.Clear();

                foreach (var launch in launches
                    .Where(l => l.LaunchpadId == siteId)
                    .OrderByDescending(l => l.DateUtc)
                    .Take(20))
                {
                    SelectedSiteLaunches.Add(launch);
                }
            }
            catch { /* Silencioso: no interrumpir UX por fallo en detalle del sitio */ }
        }
    }
}