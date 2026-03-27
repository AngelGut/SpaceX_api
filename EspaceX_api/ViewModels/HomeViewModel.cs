using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace EspaceX_api.ViewModels
{
    /// <summary>
    /// ViewModel de la vista principal (Home).
    /// Responsabilidad unica: gestionar el estado del menu de navegacion.
    ///
    /// Recibe acciones de navegacion por constructor (Dependency Inversion Principle)
    /// en lugar de recibir MainViewModel directamente, evitando dependencia circular.
    /// </summary>
    public partial class HomeViewModel : ObservableObject
    {
        private readonly Action _navigateToLaunches;
        private readonly Action _navigateToRockets;
        private readonly Action _navigateToMap;

        [ObservableProperty]
        private string title = "Explorer";

        [ObservableProperty]
        private string subtitle = "Datos en tiempo real de misiones, cohetes y sitios de lanzamiento";

        public HomeViewModel(
            Action navigateToLaunches,
            Action navigateToRockets,
            Action navigateToMap)
        {
            _navigateToLaunches = navigateToLaunches ?? throw new ArgumentNullException(nameof(navigateToLaunches));
            _navigateToRockets = navigateToRockets ?? throw new ArgumentNullException(nameof(navigateToRockets));
            _navigateToMap = navigateToMap ?? throw new ArgumentNullException(nameof(navigateToMap));
        }

        [RelayCommand]
        public void GoToLaunches() => _navigateToLaunches();

        [RelayCommand]
        public void GoToRockets() => _navigateToRockets();

        [RelayCommand]
        public void GoToMap() => _navigateToMap();
    }
}