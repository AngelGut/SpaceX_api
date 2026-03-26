using Microsoft.Extensions.DependencyInjection;
using EspaceX_api.Services;
using EspaceX_api.ViewModels;
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
        /// Configura la inyección de dependencias.
        /// Responsabilidad única: registrar servicios y ViewModels.
        /// (Dependency Inversion Principle)
        /// </summary>
        private void ConfigureServices(IServiceCollection services)
        {
            // Registrar el servicio (interfaz → implementación)
            services.AddSingleton<ISpaceXApiService, SpaceXApiService>();

            // Registrar ViewModels
            services.AddSingleton<HomeViewModel>();
            services.AddSingleton<LaunchesViewModel>();
            services.AddSingleton<RocketsViewModel>();
            services.AddSingleton<MapViewModel>();
            services.AddSingleton<MainViewModel>();

            // Registrar MainWindow
            services.AddSingleton<MainWindow>();
        }

        /// <summary>
        /// Se ejecuta al iniciar la aplicación.
        /// El contenedor DI inyecta automáticamente todas las dependencias.
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        /// <summary>
        /// Se ejecuta al cerrar la aplicación.
        /// Limpia recursos del contenedor DI.
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }

    /// <summary>
    /// Convertidor IValueConverter para visibilidad basada en string.
    /// Convierte un string vacío a Visibility.Collapsed.
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
