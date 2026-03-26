using System;

namespace EspaceX_api.Models
{
    /// <summary>
    /// Model que representa un lanzamiento.
    /// Contiene SOLO datos, sin dependencias de JSON.
    /// (Single Responsibility Principle)
    /// </summary>
    public class LaunchModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime DateUtc { get; set; }
        public string RocketId { get; set; }
        public string LaunchpadId { get; set; }
        public bool? Success { get; set; }
        public string Details { get; set; }
        public bool Upcoming { get; set; }

        /// <summary>
        /// Propiedad calculada: estado del lanzamiento.
        /// </summary>
        public string Status =>
            Upcoming ? "Próximo" :
            Success == true ? "Exitoso" :
            Success == false ? "Fallido" :
            "Desconocido";

        /// <summary>
        /// Propiedad calculada: fecha formateada.
        /// </summary>
        public string DateFormatted => DateUtc.ToString("dd/MM/yyyy HH:mm");
    }
}
