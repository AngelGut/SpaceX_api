using EspaceX_api.ViewModels;
using System.Windows.Controls;

namespace EspaceX_api.Views
{
    public partial class LaunchesView : UserControl
    {
        public LaunchesView(LaunchesViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}