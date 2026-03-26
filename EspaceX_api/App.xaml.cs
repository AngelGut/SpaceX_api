using Microsoft.Extensions.DependencyInjection;
using EspaceX_api.Services;
using EspaceX_api.ViewModels;
using EspaceX_api.Views;
using System;
using System.Windows;

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
}