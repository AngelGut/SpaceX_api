using System.Windows;
using EspaceX_api.ViewModels;

namespace EspaceX_api.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
