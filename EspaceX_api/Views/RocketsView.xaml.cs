using System.Windows.Controls;
using EspaceX_api.ViewModels;

namespace EspaceX_api
{
    public partial class RocketsView : UserControl
    {
        // Constructor sin parámetros - Porque WPF no puede instanciar la vista desde XAML sin el
        public RocketsView()
        {
            InitializeComponent();
        }

        // Constructor con DI
        public RocketsView(RocketsViewModel viewModel) : this()
        {
            DataContext = viewModel;
        }
    }
}