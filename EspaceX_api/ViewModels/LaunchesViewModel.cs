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
    public partial class LaunchesViewModel : ObservableObject
    {
        private readonly ISpaceXApiService _apiService;

        // Accion para volver al Home.
        // Se asigna desde MainViewModel via SetNavigateToHome()
        // porque DI construye este VM antes que MainViewModel.
        private Action _navigateToHome;

        [ObservableProperty] private ObservableCollection<LaunchModel> launches = new();
        [ObservableProperty] private ObservableCollection<LaunchModel> filteredLaunches = new();
        [ObservableProperty] private string searchText = string.Empty;
        [ObservableProperty] private string statusFilter = "Todos";
        [ObservableProperty] private bool isLoading = false;
        [ObservableProperty] private string errorMessage = string.Empty;

        public LaunchesViewModel(ISpaceXApiService apiService)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        }

        // Llamado desde MainViewModel despues de que DI construye este VM
        public void SetNavigateToHome(Action action) => _navigateToHome = action;

        // Comando que ejecuta el boton "Volver" en LaunchesView.xaml
        [RelayCommand]
        public void GoHome() => _navigateToHome?.Invoke();

        [RelayCommand]
        public async Task LoadLaunches()
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            try
            {
                var launches = await _apiService.GetLaunchesAsync();
                Launches = new ObservableCollection<LaunchModel>(launches);
                ApplyFilters();
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
            await LoadLaunches();
        }

        partial void OnSearchTextChanged(string value) => ApplyFilters();
        partial void OnStatusFilterChanged(string value) => ApplyFilters();

        private void ApplyFilters()
        {
            var filtered = Launches?.AsEnumerable() ?? Enumerable.Empty<LaunchModel>();

            if (!string.IsNullOrWhiteSpace(SearchText))
                filtered = filtered.Where(l =>
                    l.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                    l.Details?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true);

            if (!string.IsNullOrEmpty(StatusFilter) && StatusFilter != "Todos")
                filtered = filtered.Where(l => l.Status == StatusFilter);

            FilteredLaunches = new ObservableCollection<LaunchModel>(filtered);
        }
    }
}