using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace EspaceX_api.ViewModels
{
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

            // HomeViewModel recibe lambdas en lugar de MainViewModel directamente.
            // Esto evita la dependencia circular. (Dependency Inversion Principle)
            _homeViewModel = new HomeViewModel(
                navigateToLaunches: NavigateToLaunches,
                navigateToRockets: NavigateToRockets,
                navigateToMap: NavigateToMap
            );

            // Inyectamos la accion "Volver al Home" en cada ViewModel secundario.
            _launchesViewModel.SetNavigateToHome(NavigateToHome);
            _rocketsViewModel.SetNavigateToHome(NavigateToHome);
            _mapViewModel.SetNavigateToHome(NavigateToHome);

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
            // No carga sitios automaticamente.
            // El usuario debe presionar "Cargar Sitios" manualmente.
            CurrentViewModel = _mapViewModel;
        }

        [RelayCommand]
        public void Exit() => System.Windows.Application.Current.Shutdown();
    }
}