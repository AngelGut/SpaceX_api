namespace EspaceX_api.Models
{
    /// <summary>
    /// Model que representa un cohete.
    /// Contiene SOLO datos, sin dependencias de JSON.
    /// </summary>
    public class RocketModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public bool Active { get; set; }
        public int Stages { get; set; }
        public int Boosters { get; set; }
        public long CostPerLaunch { get; set; }
        public double SuccessRatePct { get; set; }
        public string FirstFlight { get; set; }
        public string Country { get; set; }
        public string Company { get; set; }
        public double? HeightMeters { get; set; }
        public double? HeightFeet { get; set; }
        public double? DiameterMeters { get; set; }
        public double? DiameterFeet { get; set; }
        public long MassKg { get; set; }
        public long MassLb { get; set; }
        public int EnginesNumber { get; set; }
        public string EnginesType { get; set; }
        public string Propellant1Name { get; set; }
        public string Propellant2Name { get; set; }
        public double ThrustSeaLevelKn { get; set; }
        public double ThrustVacuumKn { get; set; }
        public int? IspSeaLevel { get; set; }
        public int? IspVacuum { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// Propiedades calculadas para presentación.
        /// </summary>
        public string Status => Active ? "Activo" : "Inactivo";
        public string HeightFormatted => $"{HeightMeters}m ({HeightFeet}ft)";
        public string DiameterFormatted => $"{DiameterMeters}m ({DiameterFeet}ft)";
        public string MassFormatted => $"{MassKg} kg";
        public string CostFormatted => $"${CostPerLaunch:N0}";
    }
}
