using EspaceX_api.Services;
using EspaceX_api.ViewModels;
using EspaceX_api.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EspaceX_api
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// Configura la inyeccion de dependencias.
        /// Nota: HomeViewModel NO se registra aqui porque MainViewModel
        /// lo construye internamente pasandole lambdas de navegacion,
        /// evitando la dependencia circular.
        /// (Dependency Inversion Principle)
        /// </summary>
        private void ConfigureServices(IServiceCollection services)
        {
            // Servicio (interfaz → implementacion)
            services.AddSingleton<ISpaceXApiService, SpaceXApiService>();

            // ViewModels (HomeViewModel lo crea MainViewModel internamente)
            services.AddSingleton<LaunchesViewModel>();
            services.AddSingleton<RocketsViewModel>();
            services.AddSingleton<MapViewModel>();
            services.AddSingleton<MainViewModel>();

            // Ventana principal
            services.AddSingleton<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }

    /// <summary>
    /// Convierte un string vacio a Visibility.Collapsed.
    /// Usado en las Views para mostrar/ocultar mensajes de error.
    /// </summary>
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrWhiteSpace(value?.ToString())
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}