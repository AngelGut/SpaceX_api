# 📖 Referencia de API

Documentación técnica de todos los servicios, ViewModels y modelos.

---

## Servicios

### ISpaceXApiService

**Interfaz que define el contrato para acceder a datos de SpaceX.**

#### Métodos

##### `GetLaunchesAsync()`

```csharp
Task<List<LaunchModel>> GetLaunchesAsync()
```

**Descripción:** Obtiene lista completa de lanzamientos de SpaceX.

**Parámetros:** Ninguno

**Retorna:** `List<LaunchModel>` ordenada por fecha descendente

**Caché:** Sí (1 hora)

**Ejemplo:**
```csharp
var launches = await _apiService.GetLaunchesAsync();
foreach (var launch in launches)
{
    Console.WriteLine($"{launch.Name} - {launch.Status}");
}
```

---

##### `GetRocketsAsync()`

```csharp
Task<List<RocketModel>> GetRocketsAsync()
```

**Descripción:** Obtiene lista de todos los cohetes.

**Parámetros:** Ninguno

**Retorna:** `List<RocketModel>`

**Caché:** Sí (1 hora)

**Ejemplo:**
```csharp
var rockets = await _apiService.GetRocketsAsync();
var active = rockets.Where(r => r.Active).ToList();
```

---

##### `GetLaunchpadAsync(string launchpadId)`

```csharp
Task<LaunchpadModel> GetLaunchpadAsync(string launchpadId)
```

**Descripción:** Obtiene información detallada de una plataforma de lanzamiento.

**Parámetros:**
- `launchpadId` (string): ID único de la plataforma. Ejemplo: "5e9e4501b35332000604b32c"

**Retorna:** `LaunchpadModel` o null si no existe

**Excepciones:** `InvalidOperationException` si hay error en la API

**Ejemplo:**
```csharp
var launchpad = await _apiService.GetLaunchpadAsync("5e9e4501b35332000604b32c");
if (launchpad != null)
{
    Console.WriteLine($"{launchpad.Name}: {launchpad.LaunchAttempts} intentos");
}
```

---

##### `GetRocketAsync(string rocketId)`

```csharp
Task<RocketModel> GetRocketAsync(string rocketId)
```

**Descripción:** Obtiene información detallada de un cohete específico.

**Parámetros:**
- `rocketId` (string): ID único del cohete.

**Retorna:** `RocketModel` o null si no existe

**Ejemplo:**
```csharp
var falcon9 = await _apiService.GetRocketAsync("5e9d0d95eda30000702c38fa");
Console.WriteLine($"Falcon 9: {falcon9.HeightFormatted} de altura");
```

---

##### `ClearCache()`

```csharp
void ClearCache()
```

**Descripción:** Limpia todos los datos cacheados en memoria.

**Parámetros:** Ninguno

**Retorna:** void

**Casos de uso:**
- Usuario presiona botón "Actualizar"
- Datos están desactualizados
- Fuerza siguiente llamada a hacer request

**Ejemplo:**
```csharp
_apiService.ClearCache();
var freshLaunches = await _apiService.GetLaunchesAsync(); // Hace request
```

---

##### `IsCacheExpired()`

```csharp
bool IsCacheExpired()
```

**Descripción:** Verifica si el caché de lanzamientos ha expirado (> 1 hora).

**Parámetros:** Ninguno

**Retorna:** `true` si expiró, `false` si sigue válido

**Ejemplo:**
```csharp
if (_apiService.IsCacheExpired())
{
    _apiService.ClearCache();
}
```

---

## ViewModels

### MainViewModel

**Coordinador central de navegación y estado global.**

```csharp
public partial class MainViewModel : ObservableObject
```

#### Propiedades

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `CurrentViewModel` | ObservableObject | ViewModel actualmente visible |

#### Comandos

| Comando | Descripción |
|---------|-------------|
| `NavigateToHomeCommand` | Navega a HomeView |
| `NavigateToLaunchesCommand` | Navega a LaunchesView |
| `NavigateToRocketsCommand` | Navega a RocketsView |
| `NavigateToMapCommand` | Navega a MapView |
| `ExitCommand` | Cierra la aplicación |

#### Constructor

```csharp
public MainViewModel(
    HomeViewModel homeViewModel,
    LaunchesViewModel launchesViewModel,
    RocketsViewModel rocketsViewModel,
    MapViewModel mapViewModel)
```

---

### LaunchesViewModel

**Gestiona el estado y lógica de la pestaña de lanzamientos.**

```csharp
public partial class LaunchesViewModel : ObservableObject
```

#### Propiedades

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `Launches` | ObservableCollection<LaunchModel> | Todos los lanzamientos |
| `FilteredLaunches` | ObservableCollection<LaunchModel> | Lanzamientos filtrados |
| `SearchText` | string | Texto de búsqueda |
| `StatusFilter` | string | Filtro de estado (Todos/Próximo/Exitoso/Fallido) |
| `IsLoading` | bool | Indica si se está cargando |
| `ErrorMessage` | string | Mensaje de error (si hay) |

#### Comandos

| Comando | Descripción |
|---------|-------------|
| `LoadLaunchesCommand` | Carga lanzamientos de la API |
| `RefreshCommand` | Limpia caché y recarga |

#### Ejemplo de Uso

```csharp
var vm = new LaunchesViewModel(apiService);

// Cargar
await vm.LoadLaunchesCommand.ExecuteAsync(null);

// Filtrar
vm.SearchText = "Falcon"; // Ejecuta OnSearchTextChanged automáticamente
vm.StatusFilter = "Exitoso";

// Ver resultados
foreach (var launch in vm.FilteredLaunches)
{
    Console.WriteLine(launch.Name);
}
```

---

### RocketsViewModel

**Gestiona el estado y lógica de la pestaña de cohetes.**

```csharp
public partial class RocketsViewModel : ObservableObject
```

#### Propiedades

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `Rockets` | ObservableCollection<RocketModel> | Lista de cohetes |
| `SelectedRocket` | RocketModel | Cohete actualmente seleccionado |
| `IsLoading` | bool | Indica si se está cargando |
| `ErrorMessage` | string | Mensaje de error (si hay) |

#### Comandos

| Comando | Descripción |
|---------|-------------|
| `LoadRocketsCommand` | Carga cohetes de la API |
| `RefreshCommand` | Limpia caché y recarga |

#### Ejemplo de Uso

```csharp
var vm = new RocketsViewModel(apiService);
await vm.LoadRocketsCommand.ExecuteAsync(null);

var falcon9 = vm.Rockets.FirstOrDefault(r => r.Name == "Falcon 9");
vm.SelectedRocket = falcon9;

Console.WriteLine(falcon9.HeightFormatted);
Console.WriteLine(falcon9.CostFormatted);
```

---

### MapViewModel

**Gestiona el estado y lógica del mapa interactivo.**

```csharp
public partial class MapViewModel : ObservableObject
```

#### Propiedades

| Propiedad | Tipo | Descripción |
|-----------|------|-------------|
| `LaunchSites` | ObservableCollection<MapPointModel> | Sitios de lanzamiento |
| `SelectedSite` | MapPointModel | Sitio actualmente seleccionado |
| `SelectedSiteLaunches` | ObservableCollection<LaunchModel> | Lanzamientos del sitio |
| `ZoomLevel` | double | Nivel de zoom (0.5 a 3.0) |
| `PanX` | double | Desplazamiento horizontal |
| `PanY` | double | Desplazamiento vertical |
| `IsLoading` | bool | Indica si se está cargando |
| `ErrorMessage` | string | Mensaje de error (si hay) |

#### Comandos

| Comando | Descripción |
|---------|-------------|
| `LoadLaunchSitesCommand` | Carga sitios de lanzamiento |
| `ZoomInCommand` | Aumenta zoom en 0.2 |
| `ZoomOutCommand` | Disminuye zoom en 0.2 |
| `ResetViewCommand` | Resetea zoom y pan a valores iniciales |

#### Métodos

```csharp
public (double X, double Y) GeographicToScreenCoordinates(
    double latitude, 
    double longitude, 
    double canvasWidth, 
    double canvasHeight)
```

**Descripción:** Convierte coordenadas geográficas a coordenadas de pantalla.

**Parámetros:**
- `latitude`: Latitud (-90 a 90)
- `longitude`: Longitud (-180 a 180)
- `canvasWidth`: Ancho del canvas en píxeles
- `canvasHeight`: Alto del canvas en píxeles

**Retorna:** Tupla con (screenX, screenY)

**Ejemplo:**
```csharp
var (x, y) = mapVM.GeographicToScreenCoordinates(-33.9249, 18.4241, 800, 600);
// Dibuja punto rojo en (x, y) para Ciudad del Cabo
```

---

## Modelos de Datos

### LaunchModel

Representa un lanzamiento de SpaceX.

```csharp
public class LaunchModel
{
    public string Id { get; set; }                    // ID único
    public string Name { get; set; }                  // Nombre del lanzamiento
    public DateTime DateUtc { get; set; }            // Fecha/hora UTC
    public string RocketId { get; set; }             // ID del cohete usado
    public string LaunchpadId { get; set; }          // ID de plataforma
    public bool? Success { get; set; }               // null=pendiente, true=exitoso, false=fallido
    public string Details { get; set; }              // Descripción
    public bool Upcoming { get; set; }               // ¿Es un lanzamiento futuro?
    
    public string Status { get; }                    // "Próximo", "Exitoso", "Fallido"
    public string DateFormatted { get; }             // "dd/MM/yyyy HH:mm"
}
```

---

### RocketModel

Representa un cohete.

```csharp
public class RocketModel
{
    public string Id { get; set; }
    public string Name { get; set; }                 // Ej: "Falcon 9"
    public string Type { get; set; }
    public bool Active { get; set; }                 // ¿Está activo?
    public int Stages { get; set; }                  // Número de etapas
    public int Boosters { get; set; }
    public long CostPerLaunch { get; set; }         // Costo en USD
    public double SuccessRatePct { get; set; }      // Porcentaje de éxito
    public string FirstFlight { get; set; }          // Fecha primer vuelo
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
    public string Description { get; set; }
    
    public string Status { get; }                    // "Activo" / "Inactivo"
    public string HeightFormatted { get; }          // "70.0m (229.6ft)"
    public string CostFormatted { get; }            // "$62,000,000"
}
```

---

### LaunchpadModel

Representa una plataforma de lanzamiento.

```csharp
public class LaunchpadModel
{
    public string Id { get; set; }
    public string Name { get; set; }                 // Ej: "Kennedy Space Center LC-39A"
    public double Latitude { get; set; }            // Para mapas
    public double Longitude { get; set; }            // Para mapas
    public string Region { get; set; }               // Ej: "Florida"
    public string FullName { get; set; }
    public int LaunchAttempts { get; set; }         // Total de intentos
    public int LaunchSuccesses { get; set; }        // Intentos exitosos
}
```

---

### MapPointModel

Representa un sitio de lanzamiento en el mapa.

```csharp
public class MapPointModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int LaunchCount { get; set; }            // Total lanzamientos
    public int SuccessCount { get; set; }           // Lanzamientos exitosos
    
    public string Info { get; }                     // "{Name}\n{LaunchCount} lanzamientos..."
}
```

---

## Excepciones

### InvalidOperationException

```csharp
throw new InvalidOperationException("Error al obtener lanzamientos de la API", innerException);
```

Se lanza cuando hay un error comunicando con la API de SpaceX.

---

**[Volver a README →](../README.md)**
