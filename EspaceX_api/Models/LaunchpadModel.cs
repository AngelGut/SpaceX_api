namespace EspaceX_api.Models
{
    /// <summary>
    /// Model que representa una plataforma de lanzamiento.
    /// Contiene SOLO datos, sin dependencias de JSON.
    /// </summary>
    public class LaunchpadModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Region { get; set; }
        public string FullName { get; set; }
        public int LaunchAttempts { get; set; }
        public int LaunchSuccesses { get; set; }
    }
}
