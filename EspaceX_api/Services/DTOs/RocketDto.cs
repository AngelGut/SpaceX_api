using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EspaceX_api.Services.DTOs
{
    /// <summary>
    /// DTO para deserializar Rocket desde la API.
    /// Contiene propiedades complejas anidadas.
    /// </summary>
    public class RocketDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("active")]
        public bool Active { get; set; }

        [JsonPropertyName("stages")]
        public int Stages { get; set; }

        [JsonPropertyName("boosters")]
        public int Boosters { get; set; }

        [JsonPropertyName("cost_per_launch")]
        public long CostPerLaunch { get; set; }

        [JsonPropertyName("success_rate_pct")]
        public double SuccessRatePct { get; set; }

        [JsonPropertyName("first_flight")]
        public string FirstFlight { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("company")]
        public string Company { get; set; }

        [JsonPropertyName("height")]
        public DimensionDto Height { get; set; }

        [JsonPropertyName("diameter")]
        public DimensionDto Diameter { get; set; }

        [JsonPropertyName("mass")]
        public MassDto Mass { get; set; }

        [JsonPropertyName("payload_weights")]
        public List<PayloadWeightDto> PayloadWeights { get; set; }

        [JsonPropertyName("engines")]
        public EnginesDto Engines { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }

    public class DimensionDto
    {
        [JsonPropertyName("meters")]
        public double? Meters { get; set; }

        [JsonPropertyName("feet")]
        public double? Feet { get; set; }
    }

    public class MassDto
    {
        [JsonPropertyName("kg")]
        public long Kg { get; set; }

        [JsonPropertyName("lb")]
        public long Lb { get; set; }
    }

    public class PayloadWeightDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("kg")]
        public long Kg { get; set; }

        [JsonPropertyName("lb")]
        public long Lb { get; set; }
    }

    public class EnginesDto
    {
        [JsonPropertyName("number")]
        public int Number { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("engine_loss_max")]
        public int EngineLossMax { get; set; }

        [JsonPropertyName("propellant_1")]
        public PropellantDto Propellant1 { get; set; }

        [JsonPropertyName("propellant_2")]
        public PropellantDto Propellant2 { get; set; }

        [JsonPropertyName("thrust_sea_level")]
        public ThrustDto ThrustSeaLevel { get; set; }

        [JsonPropertyName("thrust_vacuum")]
        public ThrustDto ThrustVacuum { get; set; }

        [JsonPropertyName("isp_sea_level")]
        public int? IspSeaLevel { get; set; }

        [JsonPropertyName("isp_vacuum")]
        public int? IspVacuum { get; set; }
    }

    public class PropellantDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }

    public class ThrustDto
    {
        [JsonPropertyName("kN")]
        public double KiloNewtons { get; set; }

        [JsonPropertyName("lbf")]
        public double PoundsForce { get; set; }
    }
}
