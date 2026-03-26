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
    /// Responsabilidad única: gestionar datos y estado del mapa.
    /// No tiene ninguna referencia a controles UI (Canvas, etc.).
    /// (Single Responsibility Principle + Dependency Inversion)
    /// </summary>
    public partial class MapViewModel : ObservableObject
    {
        private readonly ISpaceXApiService _apiService;

        // ─── Propiedades observables ───────────────────────────────────────────

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

        // ─── Constantes de zoom ────────────────────────────────────────────────
        private const double ZoomStep = 0.2;
        private const double ZoomMin = 0.5;
        private const double ZoomMax = 5.0;

        // ─── Constructor ───────────────────────────────────────────────────────
        public MapViewModel(ISpaceXApiService apiService)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        }

        // ─── Reacción a cambio de sitio seleccionado ───────────────────────────
        partial void OnSelectedSiteChanged(MapPointModel? value)
        {
            SelectedSiteLaunches.Clear();
            if (value == null) return;
            _ = LoadLaunchesForSiteAsync(value.Id);
        }

        // ─── Comandos ──────────────────────────────────────────────────────────

        [RelayCommand]
        private async Task LoadLaunchSites()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                LaunchSites.Clear();

                // 1. Obtenemos todos los lanzamientos (tiene caché de 1 hora)
                var launches = await _apiService.GetLaunchesAsync();

                // 2. Agrupamos los IDs de plataformas únicas desde los lanzamientos
                var launchpadIds = launches
                    .Where(l => !string.IsNullOrEmpty(l.LaunchpadId))
                    .Select(l => l.LaunchpadId)
                    .Distinct()
                    .ToList();

                // 3. Por cada plataforma única pedimos su detalle con GetLaunchpadAsync
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

        // ─── Método público: proyección Mercator ───────────────────────────────
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

        // ─── Carga lanzamientos del sitio seleccionado ─────────────────────────
        private async Task LoadLaunchesForSiteAsync(string siteId)
        {
            try
            {
                var launches = await _apiService.GetLaunchesAsync();

                SelectedSiteLaunches.Clear();

                // Filtramos usando LaunchpadId (nombre real de la propiedad en LaunchModel)
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