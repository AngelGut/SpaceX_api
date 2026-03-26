using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EspaceX_api.ViewModels;
using System;

namespace EspaceX_api.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        /// <summary>
        /// ViewModel principal que coordina navegación entre vistas.
        /// Responsabilidad única: gestionar la vista activa.
        /// (Single Responsibility Principle)
        /// </summary>
        
        [ObservableProperty]
        private ObservableObject currentViewModel;

        private readonly HomeViewModel _homeViewModel;
        private readonly LaunchesViewModel _launchesViewModel;
        private readonly RocketsViewModel _rocketsViewModel;
        private readonly MapViewModel _mapViewModel;

        public MainViewModel(
            HomeViewModel homeViewModel,
            LaunchesViewModel launchesViewModel,
            RocketsViewModel rocketsViewModel,
            MapViewModel mapViewModel)
        {
            _homeViewModel = homeViewModel ?? throw new ArgumentNullException(nameof(homeViewModel));
            _launchesViewModel = launchesViewModel ?? throw new ArgumentNullException(nameof(launchesViewModel));
            _rocketsViewModel = rocketsViewModel ?? throw new ArgumentNullException(nameof(rocketsViewModel));
            _mapViewModel = mapViewModel ?? throw new ArgumentNullException(nameof(mapViewModel));

            CurrentViewModel = _homeViewModel;
        }

        [RelayCommand]
        public void NavigateToHome() => CurrentViewModel = _homeViewModel;

        [RelayCommand]
        public void NavigateToLaunches()
        {
            CurrentViewModel = _launchesViewModel;
            _launchesViewModel.LoadLaunchesCommand.Execute(null);
        }

        [RelayCommand]
        public void NavigateToRockets()
        {
            CurrentViewModel = _rocketsViewModel;
            _rocketsViewModel.LoadRocketsCommand.Execute(null);
        }

        [RelayCommand]
        public void NavigateToMap()
        {
            CurrentViewModel = _mapViewModel;
            _mapViewModel.LoadLaunchSitesCommand.Execute(null);
        }

        [RelayCommand]
        public void Exit() => System.Windows.Application.Current.Shutdown();
    }
}