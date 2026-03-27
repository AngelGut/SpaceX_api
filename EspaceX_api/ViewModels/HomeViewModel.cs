using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace EspaceX_api.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        private readonly MainViewModel _mainViewModel;

        [ObservableProperty]
        private string title = "Explorer";

        [ObservableProperty]
        private string subtitle = "Datos en tiempo real de misiones, cohetes y sitios de lanzamiento";

        public HomeViewModel(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));
        }

        [RelayCommand]
        public void GoToLaunches() => _mainViewModel.NavigateToLaunchesCommand.Execute(null);

        [RelayCommand]
        public void GoToRockets() => _mainViewModel.NavigateToRocketsCommand.Execute(null);

        [RelayCommand]
        public void GoToMap() => _mainViewModel.NavigateToMapCommand.Execute(null);
    }
}