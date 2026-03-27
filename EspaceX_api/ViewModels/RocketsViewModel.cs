using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EspaceX_api.Models;
using EspaceX_api.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace EspaceX_api.ViewModels
{
    public partial class RocketsViewModel : ObservableObject
    {
        private readonly ISpaceXApiService _apiService;

        // Accion para volver al Home.
        // Se asigna desde MainViewModel via SetNavigateToHome()
        // porque DI construye este VM antes que MainViewModel.
        private Action _navigateToHome;

        [ObservableProperty] private ObservableCollection<RocketModel> rockets = new();
        [ObservableProperty] private RocketModel selectedRocket;
        [ObservableProperty] private bool isLoading = false;
        [ObservableProperty] private string errorMessage = string.Empty;

        public RocketsViewModel(ISpaceXApiService apiService)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        }

        // Llamado desde MainViewModel despues de que DI construye este VM
        public void SetNavigateToHome(Action action) => _navigateToHome = action;

        // Comando que ejecuta el boton "Volver" en RocketsView.xaml
        [RelayCommand]
        public void GoHome() => _navigateToHome?.Invoke();

        [RelayCommand]
        public async Task LoadRockets()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var rockets = await _apiService.GetRocketsAsync();
                Rockets = new ObservableCollection<RocketModel>(rockets);
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

        [RelayCommand]
        public async Task Refresh()
        {
            _apiService.ClearCache();
            await LoadRockets();
        }
    }
}