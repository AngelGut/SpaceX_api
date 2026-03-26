using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EspaceX_api.Models;
using EspaceX_api.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace EspaceX_api.ViewModels
{
    /// <summary>
    /// ViewModel para la vista de cohetes.
    /// Responsabilidad única: gestionar estado y lógica de cohetes.
    /// (Single Responsibility Principle)
    /// 
    /// Asignado a: PERSONA 3
    /// </summary>
    public partial class RocketsViewModel : ObservableObject
    {
        private readonly ISpaceXApiService _apiService;

        [ObservableProperty]
        private ObservableCollection<RocketModel> rockets = new();

        [ObservableProperty]
        private RocketModel selectedRocket;

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        public RocketsViewModel(ISpaceXApiService apiService)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        }

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
