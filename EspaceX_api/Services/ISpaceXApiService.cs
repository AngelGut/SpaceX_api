using EspaceX_api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EspaceX_api.Services
{
    /// <summary>
    /// Contrato para el servicio de SpaceX API.
    /// Responsabilidad única: obtener y mapear datos de la API.
    /// (Interface Segregation Principle)
    /// </summary>
    public interface ISpaceXApiService
    {
        /// <summary>
        /// Obtiene lista de lanzamientos con caché de 1 hora.
        /// </summary>
        Task<List<LaunchModel>> GetLaunchesAsync();

        /// <summary>
        /// Obtiene lista de cohetes con caché de 1 hora.
        /// </summary>
        Task<List<RocketModel>> GetRocketsAsync();

        /// <summary>
        /// Obtiene información de una plataforma de lanzamiento.
        /// </summary>
        Task<LaunchpadModel> GetLaunchpadAsync(string launchpadId);

        /// <summary>
        /// Obtiene información de un cohete específico.
        /// </summary>
        Task<RocketModel> GetRocketAsync(string rocketId);

        /// <summary>
        /// Limpia el caché en memoria.
        /// </summary>
        void ClearCache();

        /// <summary>
        /// Verifica si el caché está expirado.
        /// </summary>
        bool IsCacheExpired();
    }
}
