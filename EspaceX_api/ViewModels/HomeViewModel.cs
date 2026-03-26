using CommunityToolkit.Mvvm.ComponentModel;

namespace EspaceX_api.ViewModels
{
    /// <summary>
    /// ViewModel para la vista de inicio/menú.
    /// Responsabilidad única: gestionar el estado del menú home.
    /// (Single Responsibility Principle)
    /// 
    /// Asignado a: PERSONA 1
    /// </summary>
    public partial class HomeViewModel : ObservableObject
    {
        [ObservableProperty]
        private string title = "SpaceX Explorer";

        [ObservableProperty]
        private string subtitle = "Explora datos en tiempo real de SpaceX";

        public HomeViewModel()
        {
        }
    }
}
