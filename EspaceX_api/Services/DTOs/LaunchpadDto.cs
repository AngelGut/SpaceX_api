using System.Text.Json.Serialization;

namespace EspaceX_api.Services.DTOs
{
    /// <summary>
    /// DTO para deserializar Launchpad desde la API.
    /// </summary>
    public class LaunchpadDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("region")]
        public string Region { get; set; }

        [JsonPropertyName("full_name")]
        public string FullName { get; set; }

        [JsonPropertyName("launch_attempts")]
        public int LaunchAttempts { get; set; }

        [JsonPropertyName("launch_successes")]
        public int LaunchSuccesses { get; set; }
    }
}
