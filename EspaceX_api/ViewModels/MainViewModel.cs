using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace EspaceX_api.ViewModels
{
    /// <summary>
    /// ViewModel principal que coordina la navegacion entre vistas.
    /// Responsabilidad unica: gestionar la vista activa (CurrentViewModel).
    ///
    /// Construye HomeViewModel pasandole las acciones de navegacion como lambdas,
    /// rompiendo la dependencia circular que existia cuando HomeViewModel
    /// recibia MainViewModel directamente.
    /// (Single Responsibility + Dependency Inversion Principle)
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableObject currentViewModel;

        private readonly HomeViewModel _homeViewModel;
        private readonly LaunchesViewModel _launchesViewModel;
        private readonly RocketsViewModel _rocketsViewModel;
        private readonly MapViewModel _mapViewModel;

        public MainViewModel(
            LaunchesViewModel launchesViewModel,
            RocketsViewModel rocketsViewModel,
            MapViewModel mapViewModel)
        {
            _launchesViewModel = launchesViewModel ?? throw new ArgumentNullException(nameof(launchesViewModel));
            _rocketsViewModel = rocketsViewModel ?? throw new ArgumentNullException(nameof(rocketsViewModel));
            _mapViewModel = mapViewModel ?? throw new ArgumentNullException(nameof(mapViewModel));

            // HomeViewModel recibe lambdas, no una referencia a MainViewModel.
            // Esto rompe la dependencia circular.
            _homeViewModel = new HomeViewModel(
                navigateToLaunches: NavigateToLaunches,
                navigateToRockets: NavigateToRockets,
                navigateToMap: NavigateToMap
            );

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
