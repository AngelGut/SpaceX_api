using System.Windows.Controls;
using EspaceX_api.ViewModels;

namespace EspaceX_api.Views
{
    /// <summary>
    /// Code-behind de RocketsView.
    /// Responsabilidad unica: inicializar el componente.
    /// El DataContext llega automaticamente desde el DataTemplate en MainWindow.
    /// </summary>
    public partial class RocketsView : UserControl
    {
        public RocketsView()
        {
            InitializeComponent();
        }
    }
}
