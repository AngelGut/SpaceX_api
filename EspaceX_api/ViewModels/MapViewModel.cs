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
    /// Modelo para representar un punto en el mapa (sitio de lanzamiento).
    /// </summary>
    public class MapPointModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int LaunchCount { get; set; }
        public int SuccessCount { get; set; }

        public string Info => $"{Name}\n{LaunchCount} lanzamientos ({SuccessCount} exitosos)";
    }

    /// <summary>
    /// ViewModel para la vista de mapa.
    /// Responsabilidades: gestionar sitios de lanzamiento y proyección de coordenadas.
    /// (Single Responsibility Principle)
    /// 
    /// Asignado a: PERSONA 4
    /// </summary>
    public partial class MapViewModel : ObservableObject
    {
        private readonly ISpaceXApiService _apiService;

        [ObservableProperty]
        private ObservableCollection<MapPointModel> launchSites = new();

        [ObservableProperty]
        private ObservableCollection<LaunchModel> selectedSiteLaunches = new();

        [ObservableProperty]
        private MapPointModel selectedSite;

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private double zoomLevel = 1.0;

        [ObservableProperty]
        private double panX = 0;

        [ObservableProperty]
        private double panY = 0;

        public MapViewModel(ISpaceXApiService apiService)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        }

        [RelayCommand]
        public async Task LoadLaunchSites()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var launches = await _apiService.GetLaunchesAsync();
                var sitesDict = new Dictionary<string, MapPointModel>();

                foreach (var launch in launches)
                {
                    if (string.IsNullOrEmpty(launch.LaunchpadId))
                        continue;

                    if (!sitesDict.ContainsKey(launch.LaunchpadId))
                    {
                        var launchpad = await _apiService.GetLaunchpadAsync(launch.LaunchpadId);
                        if (launchpad != null)
                        {
                            sitesDict[launch.LaunchpadId] = new MapPointModel
                            {
                                Id = launchpad.Id,
                                Name = launchpad.Name,
                                Latitude = launchpad.Latitude,
                                Longitude = launchpad.Longitude,
                                LaunchCount = 0,
                                SuccessCount = 0
                            };
                        }
                    }

                    if (sitesDict.ContainsKey(launch.LaunchpadId))
                    {
                        var point = sitesDict[launch.LaunchpadId];
                        point.LaunchCount++;
                        if (launch.Success == true)
                            point.SuccessCount++;
                    }
                }

                LaunchSites = new ObservableCollection<MapPointModel>(
                    sitesDict.Values.OrderBy(s => s.Name)
                );
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        partial void OnSelectedSiteChanged(MapPointModel value)
        {
            LoadSiteLaunches(value?.Id);
        }

        [RelayCommand]
        public void ZoomIn()
        {
            ZoomLevel = Math.Min(ZoomLevel + 0.2, 3.0);
        }

        [RelayCommand]
        public void ZoomOut()
        {
            ZoomLevel = Math.Max(ZoomLevel - 0.2, 0.5);
        }

        [RelayCommand]
        public void ResetView()
        {
            ZoomLevel = 1.0;
            PanX = 0;
            PanY = 0;
        }

        private async void LoadSiteLaunches(string launchpadId)
        {
            if (string.IsNullOrEmpty(launchpadId))
            {
                SelectedSiteLaunches.Clear();
                return;
            }

            try
            {
                var launches = await _apiService.GetLaunchesAsync();
                var siteLaunches = launches
                    .Where(l => l.LaunchpadId == launchpadId)
                    .OrderByDescending(l => l.DateUtc)
                    .ToList();

                SelectedSiteLaunches = new ObservableCollection<LaunchModel>(siteLaunches);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Convierte coordenadas geográficas a coordenadas de pantalla.
        /// Usa proyección Mercator simplificada.
        /// </summary>
        public (double X, double Y) GeographicToScreenCoordinates(double latitude, double longitude,
            double canvasWidth, double canvasHeight)
        {
            const double maxLat = 85.051129;
            const double maxLon = 180;

            double x = (longitude + maxLon) / (maxLon * 2);
            double y = (maxLat - latitude) / (maxLat * 2);

            double screenX = (x * canvasWidth * ZoomLevel) + PanX;
            double screenY = (y * canvasHeight * ZoomLevel) + PanY;

            return (screenX, screenY);
        }
    }
}
