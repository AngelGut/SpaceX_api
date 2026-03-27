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
    /// <summary>
    /// ViewModel del mapa interactivo de sitios de lanzamiento.
    /// Responsabilidad única: gestionar datos y estado del mapa (zoom, pan, sitios, lanzamientos).
    /// No tiene ninguna referencia a controles UI (Canvas, Mouse, etc.).
    /// (Single Responsibility Principle + Dependency Inversion)
    /// </summary>
    public partial class MapViewModel : ObservableObject
    {
        private readonly ISpaceXApiService _apiService;

        
        private const double ZoomStep = 0.2;
        private const double ZoomMin = 0.5;
        private const double ZoomMax = 5.0;

        private bool _isDragging = false;
        private double _lastDragX = 0;
        private double _lastDragY = 0;

        
        [ObservableProperty]
        private ObservableCollection<MapPointModel> launchSites = new();

        [ObservableProperty]
        private MapPointModel? selectedSite;

        [ObservableProperty]
        private ObservableCollection<LaunchModel> selectedSiteLaunches = new();

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private double zoomLevel = 1.0;

        [ObservableProperty]
        private double panX = 0;

        [ObservableProperty]
        private double panY = 0;

        /// <summary>
        /// Recibe ISpaceXApiService por inyección de dependencias.
        /// (Dependency Inversion Principle)
        /// </summary>
        public MapViewModel(ISpaceXApiService apiService)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        }

        //TODO: Cargar sitios al inicializar el ViewModel, o dejar que la View lo haga al cargar

        partial void OnSelectedSiteChanged(MapPointModel? value)
        {
            SelectedSiteLaunches.Clear();
            if (value == null) return;
            _ = LoadLaunchesForSiteAsync(value.Id);
        }

        //TODO: Comando para recargar sitios manualmente, por si el usuario quiere actualizar la información sin reiniciar la app.

        /// <summary>
        /// Carga todos los sitios de lanzamiento desde la API y los convierte en MapPointModel.
        /// </summary>
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
                    catch
                    {
                        // Si falla un pad individual, continuamos con los demás
                    }
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

        //TODO: Comandos para zoom in/out y reset view, que ajusten ZoomLevel y PanX/PanY respectivamente.

        [RelayCommand]
        private void ZoomIn()
        {
            ZoomLevel = Math.Min(ZoomLevel + ZoomStep, ZoomMax);
        }

        [RelayCommand]
        private void ZoomOut()
        {
            ZoomLevel = Math.Max(ZoomLevel - ZoomStep, ZoomMin);
        }

        [RelayCommand]
        private void ResetView()
        {
            ZoomLevel = 1.0;
            PanX = 0;
            PanY = 0;
        }

        //TODO: Metodo para manejar drag del mapa, que actualice PanX/PanY según el movimiento del mouse. La View llamará esto desde los eventos de mouse.

        /// <summary>
        /// Notifica al ViewModel que el usuario empezó a arrastrar el mapa.
        /// La View llama esto desde MouseLeftButtonDown.
        /// (SRP: el estado del drag vive aquí, no en el code-behind)
        /// </summary>
        public void BeginDrag(double x, double y)
        {
            _isDragging = true;
            _lastDragX = x;
            _lastDragY = y;
        }

        /// <summary>
        /// Notifica al ViewModel que el usuario soltó el mouse.
        /// La View llama esto desde MouseLeftButtonUp.
        /// </summary>
        public void EndDrag()
        {
            _isDragging = false;
        }

        /// <summary>
        /// Actualiza PanX/PanY según el movimiento del mouse.
        /// La View solo pasa las coordenadas actuales; el ViewModel calcula el delta.
        /// (SRP: lógica de negocio del pan centralizada aquí)
        /// </summary>
        public void UpdateDrag(double currentX, double currentY)
        {
            if (!_isDragging) return;

            PanX += currentX - _lastDragX;
            PanY += currentY - _lastDragY;

            _lastDragX = currentX;
            _lastDragY = currentY;
        }

        //TODO: Conversion de coordenadas geográficas a pantalla, aplicando proyección Mercator + zoom + pan. La View la usará para posicionar los puntos en el Canvas.

        /// <summary>
        /// Convierte coordenadas geográficas a coordenadas de pantalla
        /// aplicando proyección Mercator + zoom + pan.
        /// Es público para que MapView.xaml.cs lo use al dibujar.
        /// </summary>
        public (double x, double y) GeographicToScreenCoordinates(
            double latitude, double longitude, double canvasWidth, double canvasHeight)
        {
            double x = (longitude + 180.0) / 360.0;
            double latRad = latitude * Math.PI / 180.0;
            double mercN = Math.Log(Math.Tan(Math.PI / 4 + latRad / 2));
            double y = 0.5 - mercN / (2 * Math.PI);

            double screenX = x * canvasWidth * ZoomLevel + PanX;
            double screenY = y * canvasHeight * ZoomLevel + PanY;

            return (screenX, screenY);
        }

        //TODO: Cargar lanzamientos para el sitio seleccionado, filtrando por LaunchpadId. Esto se llama automáticamente cuando cambia SelectedSite.

        /// <summary>
        /// Filtra los lanzamientos del sitio seleccionado y los carga en SelectedSiteLaunches.
        /// </summary>
        private async Task LoadLaunchesForSiteAsync(string siteId)
        {
            try
            {
                var launches = await _apiService.GetLaunchesAsync();

                SelectedSiteLaunches.Clear();

                var filtered = launches
                    .Where(l => l.LaunchpadId == siteId)
                    .OrderByDescending(l => l.DateUtc)
                    .Take(20);

                foreach (var launch in filtered)
                    SelectedSiteLaunches.Add(launch);
            }
            catch
            {
                // Silencioso: no interrumpir UX por fallo en detalle del sitio
            }
        }
    }
}