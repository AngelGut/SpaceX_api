using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EspaceX_api.Models
{
    /// <summary>
    /// Modelo que representa un punto en el mapa (sitio de lanzamiento).
    /// Responsabilidad única: contener los datos de un sitio geográfico.
    /// (Single Responsibility Principle)
    /// </summary>
    public class MapPointModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Info { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public int TotalLaunches { get; set; }
    }
}