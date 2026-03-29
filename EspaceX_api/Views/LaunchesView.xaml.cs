using System.Windows.Controls;

namespace EspaceX_api.Views
{
    /// <summary>
    /// Code-behind de LaunchesView.
    /// Responsabilidad unica: inicializar el componente.
    /// El DataContext llega automaticamente desde el DataTemplate en MainWindow.
    /// (Single Responsibility Principle)
    /// </summary>
    public partial class LaunchesView : UserControl
    {
        public LaunchesView()
        {
            InitializeComponent();
        }
    }
}