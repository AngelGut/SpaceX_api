using EspaceX_api.Models;
using EspaceX_api.Services.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace EspaceX_api.Services
{
    /// <summary>
    /// Implementación del servicio de SpaceX API.
    /// Responsabilidad única: 
    /// 1. Comunicar con API
    /// 2. Deserializar DTOs
    /// 3. Mapear DTOs → Models
    /// 4. Cachear resultados
    /// (Single Responsibility Principle)
    /// </summary>
    public class SpaceXApiService : ISpaceXApiService
    {
        private const string BaseUrl = "https://api.spacexdata.com/v4";
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(1);

        // Caché
        private List<LaunchModel> _launchesCache = new();
        private List<RocketModel> _rocketsCache = new();
        private Dictionary<string, LaunchpadModel> _launchpadsCache = new();
        
        private DateTime _launchesCacheTime = DateTime.MinValue;
        private DateTime _rocketsCacheTime = DateTime.MinValue;

        public SpaceXApiService()
        {
            _httpClient = new HttpClient();
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<List<LaunchModel>> GetLaunchesAsync()
        {
            if (IsCacheValid(_launchesCacheTime) && _launchesCache.Any())
                return _launchesCache;

            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/launches");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var dtos = JsonSerializer.Deserialize<List<LaunchDto>>(json, _jsonOptions) ?? new();
                
                // Mapeo: DTO → Model
                _launchesCache = dtos.Select(MapToLaunchModel).OrderByDescending(l => l.DateUtc).ToList();
                _launchesCacheTime = DateTime.UtcNow;

                return _launchesCache;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error al obtener lanzamientos", ex);
            }
        }

        public async Task<List<RocketModel>> GetRocketsAsync()
        {
            if (IsCacheValid(_rocketsCacheTime) && _rocketsCache.Any())
                return _rocketsCache;

            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/rockets");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var dtos = JsonSerializer.Deserialize<List<RocketDto>>(json, _jsonOptions) ?? new();
                
                // Mapeo: DTO → Model
                _rocketsCache = dtos.Select(MapToRocketModel).ToList();
                _rocketsCacheTime = DateTime.UtcNow;

                return _rocketsCache;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error al obtener cohetes", ex);
            }
        }

        public async Task<LaunchpadModel> GetLaunchpadAsync(string launchpadId)
        {
            if (string.IsNullOrEmpty(launchpadId))
                return null;

            if (_launchpadsCache.TryGetValue(launchpadId, out var cached))
                return cached;

            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/launchpads/{launchpadId}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var dto = JsonSerializer.Deserialize<LaunchpadDto>(json, _jsonOptions);

                if (dto != null)
                {
                    var model = MapToLaunchpadModel(dto);
                    _launchpadsCache[launchpadId] = model;
                    return model;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al obtener plataforma", ex);
            }
        }

        public async Task<RocketModel> GetRocketAsync(string rocketId)
        {
            if (string.IsNullOrEmpty(rocketId))
                return null;

            var cached = _rocketsCache.FirstOrDefault(r => r.Id == rocketId);
            if (cached != null)
                return cached;

            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/rockets/{rocketId}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var dto = JsonSerializer.Deserialize<RocketDto>(json, _jsonOptions);

                return dto != null ? MapToRocketModel(dto) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al obtener cohete", ex);
            }
        }

        public void ClearCache()
        {
            _launchesCache.Clear();
            _rocketsCache.Clear();
            _launchpadsCache.Clear();
            _launchesCacheTime = DateTime.MinValue;
            _rocketsCacheTime = DateTime.MinValue;
        }

        public bool IsCacheExpired()
        {
            return !IsCacheValid(_launchesCacheTime);
        }

        /// <summary>
        /// Mapea LaunchDto → LaunchModel.
        /// </summary>
        private LaunchModel MapToLaunchModel(LaunchDto dto)
        {
            return new LaunchModel
            {
                Id = dto.Id,
                Name = dto.Name,
                DateUtc = dto.DateUtc,
                RocketId = dto.RocketId,
                LaunchpadId = dto.LaunchpadId,
                Success = dto.Success,
                Details = dto.Details,
                Upcoming = dto.Upcoming
            };
        }

        /// <summary>
        /// Mapea RocketDto → RocketModel.
        /// </summary>
        private RocketModel MapToRocketModel(RocketDto dto)
        {
            return new RocketModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Type = dto.Type,
                Active = dto.Active,
                Stages = dto.Stages,
                Boosters = dto.Boosters,
                CostPerLaunch = dto.CostPerLaunch,
                SuccessRatePct = dto.SuccessRatePct,
                FirstFlight = dto.FirstFlight,
                Country = dto.Country,
                Company = dto.Company,
                HeightMeters = dto.Height?.Meters,
                HeightFeet = dto.Height?.Feet,
                DiameterMeters = dto.Diameter?.Meters,
                DiameterFeet = dto.Diameter?.Feet,
                MassKg = dto.Mass?.Kg ?? 0,
                MassLb = dto.Mass?.Lb ?? 0,
                EnginesNumber = dto.Engines?.Number ?? 0,
                EnginesType = dto.Engines?.Type,
                Propellant1Name = dto.Engines?.Propellant1?.Name,
                Propellant2Name = dto.Engines?.Propellant2?.Name,
                ThrustSeaLevelKn = dto.Engines?.ThrustSeaLevel?.KiloNewtons ?? 0,
                ThrustVacuumKn = dto.Engines?.ThrustVacuum?.KiloNewtons ?? 0,
                IspSeaLevel = dto.Engines?.IspSeaLevel,
                IspVacuum = dto.Engines?.IspVacuum,
                Description = dto.Description
            };
        }

        /// <summary>
        /// Mapea LaunchpadDto → LaunchpadModel.
        /// </summary>
        private LaunchpadModel MapToLaunchpadModel(LaunchpadDto dto)
        {
            return new LaunchpadModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Region = dto.Region,
                FullName = dto.FullName,
                LaunchAttempts = dto.LaunchAttempts,
                LaunchSuccesses = dto.LaunchSuccesses
            };
        }

        private bool IsCacheValid(DateTime cacheTime)
        {
            return DateTime.UtcNow - cacheTime < _cacheExpiration;
        }
    }
}
