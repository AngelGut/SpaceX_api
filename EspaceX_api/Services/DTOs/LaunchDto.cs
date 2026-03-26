using System;
using System.Text.Json.Serialization;

namespace EspaceX_api.Services.DTOs
{
    /// <summary>
    /// DTO (Data Transfer Object) para deserializar Launch desde la API.
    /// Mapeo 1:1 con JSON de SpaceX API.
    /// </summary>
    public class LaunchDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("date_utc")]
        public DateTime DateUtc { get; set; }

        [JsonPropertyName("rocket")]
        public string RocketId { get; set; }

        [JsonPropertyName("launchpad")]
        public string LaunchpadId { get; set; }

        [JsonPropertyName("success")]
        public bool? Success { get; set; }

        [JsonPropertyName("details")]
        public string Details { get; set; }

        [JsonPropertyName("upcoming")]
        public bool Upcoming { get; set; }
    }
}
