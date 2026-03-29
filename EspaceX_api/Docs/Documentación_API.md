# 📖 Documentación de API

**EspaceX_api - Referencia de Servicios, ViewModels y Modelos**

---

## 1. Interfaz ISpaceXApiService

**Ubicación:** `Services/ISpaceXApiService.cs`

**Propósito:** Contrato para acceso a datos de SpaceX

---

### GetLaunchesAsync()

```csharp
Task<List<LaunchModel>> GetLaunchesAsync()
```

| Parámetro | - |
|-----------|---|
| **Retorna** | List<LaunchModel> ordenada por fecha descendente |
| **Caché** | Sí, 1 hora |
| **Excepciones** | InvalidOperationException si falla API |

**Descripción:** Obtiene historial completo de lanzamientos de SpaceX (~150+ lanzamientos).

**Ejemplo de uso:**
```csharp
var launches = await _apiService.GetLaunchesAsync();
foreach (var launch in launches)
{
    Console.WriteLine($"{launch.Name} - {launch.Status}");
}
```

---

### GetRocketsAsync()

```csharp
Task<List<RocketModel>> GetRocketsAsync()
```

| Parámetro | - |
|-----------|---|
| **Retorna** | List<RocketModel> |
| **Caché** | Sí, 1 hora |
| **Excepciones** | InvalidOperationException si falla API |

**Descripción:** Obtiene especificaciones técnicas de todos los cohetes de SpaceX.

**Ejemplo:**
```csharp
var rockets = await _apiService.GetRocketsAsync();
var active = rockets.Where(r => r.Active).ToList();
Console.WriteLine($"Cohetes activos: {active.Count}");
```

---

### GetLaunchpadAsync(string launchpadId)

```csharp
Task<LaunchpadModel> GetLaunchpadAsync(string launchpadId)
```

| Parámetro | Descripción |
|-----------|-----------|
| **launchpadId** | ID único de plataforma (UUID). Ej: "5e9e4501b35332000604b32c" |
| **Retorna** | LaunchpadModel con datos del sitio, null si no existe |
| **Caché** | No (permanente por sesión) |
| **Excepciones** | InvalidOperationException si falla API |

**Descripción:** Obtiene datos de un sitio de lanzamiento específico (ubicación, intentos, éxitos).

**Ejemplo:**
```csharp
var launchpad = await _apiService.GetLaunchpadAsync("5e9e4501b35332000604b32c");
if (launchpad != null)
{
    Console.WriteLine($"{launchpad.Name}: {launchpad.LaunchAttempts} intentos");
}
```

---

### GetRocketAsync(string rocketId)

```csharp
Task<RocketModel> GetRocketAsync(string rocketId)
```

| Parámetro | Descripción |
|-----------|-----------|
| **rocketId** | ID único del cohete (UUID). Ej: "5e9d0d95eda30000702c38fa" |
| **Retorna** | RocketModel, null si no existe |
| **Caché** | No |
| **Excepciones** | InvalidOperationException si falla API |

**Descripción:** Obtiene datos de un cohete específico (especificaciones técnicas).

**Ejemplo:**
```csharp
var falcon9 = await _apiService.GetRocketAsync("5e9d0d95eda30000702c38fa");
if (falcon9 != null)
{
    Console.WriteLine($"Altura: {falcon9.HeightFormatted}");
    Console.WriteLine($"Costo: {falcon9.CostFormatted}");
}
```

---

### ClearCache()

```csharp
void ClearCache()
```

| Parámetro | - |
|-----------|---|
| **Retorna** | void |
| **Efecto** | Limpia _launchesCache y _rocketsCache |
| **Caché** | Resetea timestamps a DateTime.MinValue |

**Descripción:** Fuerza que la próxima llamada consulte API (no use caché).

**Casos de uso:**
- Usuario presiona botón "Actualizar"
- Datos están desactualizados
- Desarrollador necesita datos frescos en testing

**Ejemplo:**
```csharp
_apiService.ClearCache();
var freshData = await _apiService.GetLaunchesAsync(); // Consulta API nuevamente
```

---

### IsCacheExpired()

```csharp
bool IsCacheExpired()
```

| Parámetro | - |
|-----------|---|
| **Retorna** | true si caché de lanzamientos expiró (> 1 hora), false si válido |
| **Nota** | Verifica específicamente lanzamientos |

**Descripción:** Consulta si el caché actual ha expirado.

**Ejemplo:**
```csharp
if (_apiService.IsCacheExpired())
{
    _apiService.ClearCache();
    // Próxima llamada hará request
}
```

---

## 2. ViewModels

### 2.1 MainViewModel

**Ubicación:** `ViewModels/MainViewModel.cs`

**Responsabilidad:** Coordinador central de navegación y estado global.

#### Propiedades Observables

| Propiedad | Tipo | Descripción |
|-----------|------|-----------|
| `CurrentViewModel` | ObservableObject | ViewModel actualmente visible (notifica DataTemplate) |

#### Comandos

| Comando | Parámetro | Efecto |
|---------|-----------|--------|
| `NavigateToHomeCommand` | - | CurrentViewModel = _homeViewModel |
| `NavigateToLaunchesCommand` | - | CurrentViewModel = _launchesViewModel + carga datos |
| `NavigateToRocketsCommand` | - | CurrentViewModel = _rocketsViewModel + carga datos |
| `NavigateToMapCommand` | - | CurrentViewModel = _mapViewModel (sin autoload) |
| `ExitCommand` | - | Application.Current.Shutdown() |

#### Constructor

```csharp
public MainViewModel(
    LaunchesViewModel launchesViewModel,
    RocketsViewModel rocketsViewModel,
    MapViewModel mapViewModel)
```

**Nota:** HomeViewModel NO se inyecta; se construye internamente con lambdas para evitar dependencia circular.

#### Métodos Especiales

```csharp
// Inyección post-construcción en VMs secundarios
_launchesViewModel.SetNavigateToHome(NavigateToHome);
_rocketsViewModel.SetNavigateToHome(NavigateToHome);
_mapViewModel.SetNavigateToHome(NavigateToHome);
```

---

### 2.2 LaunchesViewModel

**Ubicación:** `ViewModels/LaunchesViewModel.cs`

**Responsabilidad:** Gestiona estado y lógica de pestaña de lanzamientos.

#### Propiedades Observables

| Propiedad | Tipo | Descripción |
|-----------|------|-----------|
| `Launches` | ObservableCollection<LaunchModel> | Todos los lanzamientos (sin filtrar) |
| `FilteredLaunches` | ObservableCollection<LaunchModel> | Lanzamientos filtrados (mostrados en tabla) |
| `SearchText` | string | Texto de búsqueda (observable) |
| `StatusFilter` | string | Filtro estado: "Todos", "Proximo", "Exitoso", "Fallido" |
| `IsLoading` | bool | true mientras API responde |
| `ErrorMessage` | string | Mensaje de error (vacío si sin error) |

#### Comandos

| Comando | Descripción |
|---------|-----------|
| `LoadLaunchesCommand` | Carga lanzamientos desde API, cachea, aplica filtros |
| `RefreshCommand` | Limpia caché de lanzamientos y recarga |
| `GoHomeCommand` | Ejecuta _navigateToHome?.Invoke() |

#### Observadores (Partial Methods)

```csharp
partial void OnSearchTextChanged(string value) 
    → Ejecuta ApplyFilters() automáticamente

partial void OnStatusFilterChanged(string value)
    → Ejecuta ApplyFilters() automáticamente
```

#### Método Privado: ApplyFilters()

Filtra `Launches` según `SearchText` y `StatusFilter` usando LINQ:
- **SearchText:** case-insensitive en Name y Details
- **StatusFilter:** exact match en Status property

**Asigna:** `FilteredLaunches = new ObservableCollection(...)`

#### Ejemplo de Uso

```csharp
var vm = new LaunchesViewModel(apiService);

// Cargar
await vm.LoadLaunchesCommand.ExecuteAsync(null);

// Filtrar (aplica automáticamente)
vm.SearchText = "Falcon";
vm.StatusFilter = "Exitoso";

// Leer resultados
foreach (var launch in vm.FilteredLaunches)
{
    Console.WriteLine(launch.Name);
}

// Actualizar
vm.RefreshCommand.Execute(null);
```

---

### 2.3 RocketsViewModel

**Ubicación:** `ViewModels/RocketsViewModel.cs`

**Responsabilidad:** Gestiona estado y lógica de pestaña de cohetes.

#### Propiedades Observables

| Propiedad | Tipo | Descripción |
|-----------|------|-----------|
| `Rockets` | ObservableCollection<RocketModel> | Lista de cohetes |
| `SelectedRocket` | RocketModel | Cohete actualmente seleccionado (puede ser null) |
| `IsLoading` | bool | true mientras API responde |
| `ErrorMessage` | string | Mensaje de error |

#### Comandos

| Comando | Descripción |
|---------|-----------|
| `LoadRocketsCommand` | Carga cohetes desde API, cachea |
| `RefreshCommand` | Limpia caché y recarga |
| `GoHomeCommand` | Retorna al Home |

#### Ejemplo

```csharp
var vm = new RocketsViewModel(apiService);
await vm.LoadRocketsCommand.ExecuteAsync(null);

var falcon9 = vm.Rockets.FirstOrDefault(r => r.Name == "Falcon 9");
vm.SelectedRocket = falcon9;

Console.WriteLine(falcon9.HeightFormatted);    // "70.0m (229.6ft)"
Console.WriteLine(falcon9.CostFormatted);      // "$62,000,000"
Console.WriteLine(falcon9.SuccessRatePct);     // "97.8"
```

---

### 2.4 MapViewModel

**Ubicación:** `ViewModels/MapViewModel.cs`

**Responsabilidad:** Gestiona mapa interactivo, sitios y datos de zoom/pan.

#### Propiedades Observables

| Propiedad | Tipo | Rango/Descripción |
|-----------|------|---|
| `LaunchSites` | ObservableCollection<MapPointModel> | Todos los sitios cargados |
| `SelectedSite` | MapPointModel | Sitio actualmente seleccionado (null si ninguno) |
| `SelectedSiteLaunches` | ObservableCollection<LaunchModel> | Últimos ~20 lanzamientos del sitio |
| `ZoomLevel` | double | Rango 0.5 a 5.0 (default 1.0) |
| `PanX` | double | Desplazamiento horizontal en píxeles |
| `PanY` | double | Desplazamiento vertical en píxeles |
| `IsLoading` | bool | true mientras carga sitios |
| `ErrorMessage` | string | Mensaje de error |

#### Comandos

| Comando | Descripción |
|---------|-----------|
| `LoadLaunchSitesCommand` | Carga sitios: GetLaunchesAsync → extrae launchpadIds → GetLaunchpadAsync(id) por cada uno |
| `ZoomInCommand` | ZoomLevel += 0.2 (máx 5.0) |
| `ZoomOutCommand` | ZoomLevel -= 0.2 (mín 0.5) |
| `ResetViewCommand` | ZoomLevel = 1.0; PanX = PanY = 0 |
| `GoHomeCommand` | Retorna al Home |

#### Métodos Públicos

```csharp
public (double X, double Y) GeographicToScreenCoordinates(
    double latitude,      // -90 a 90
    double longitude,     // -180 a 180
    double canvasWidth,   // píxeles
    double canvasHeight)  // píxeles
```

**Convierte:** Coordenadas geográficas → píxeles de pantalla con proyección Mercator + zoom + pan aplicados.

**Retorna:** (screenX, screenY) ready para Canvas.SetLeft/SetTop

```csharp
public void BeginDrag(double x, double y)   // Inicia arrastre
public void EndDrag()                        // Termina arrastre
public void UpdateDrag(double x, double y)  // Actualiza pan durante arrastre
```

#### Observador Automático

```csharp
partial void OnSelectedSiteChanged(MapPointModel? value)
    → Carga automáticamente SelectedSiteLaunches para el sitio
    → MapView.xaml.cs DrawMap() se ejecuta (dibuja punto)
```

#### Ejemplo

```csharp
var vm = new MapViewModel(apiService);
await vm.LoadLaunchSitesCommand.ExecuteAsync(null);
// LaunchSites ahora contiene todos los sitios

vm.SelectedSite = vm.LaunchSites.First();
// SelectedSiteLaunches se carga automáticamente

var (screenX, screenY) = vm.GeographicToScreenCoordinates(
    latitude: 28.5721,   // Kennedy Space Center
    longitude: -80.6469,
    canvasWidth: 800,
    canvasHeight: 600);
// screenX, screenY = coordenadas en Canvas
```

---

### 2.5 HomeViewModel

**Ubicación:** `ViewModels/HomeViewModel.cs`

**Responsabilidad:** Mostrar dashboard interactivo.

#### Constructor

```csharp
public HomeViewModel(
    Action navigateToLaunches,  // Lambdas inyectadas
    Action navigateToRockets,
    Action navigateToMap)
```

**Nota:** No toma ISpaceXApiService (no carga datos).

#### Comandos

| Comando | Efecto |
|---------|--------|
| `NavigateToLaunchesCommand` | navigateToLaunches?.Invoke() |
| `NavigateToRocketsCommand` | navigateToRockets?.Invoke() |
| `NavigateToMapCommand` | navigateToMap?.Invoke() |

#### Ejemplo

```csharp
var homeVm = new HomeViewModel(
    navigateToLaunches: () => Console.WriteLine("→ Launches"),
    navigateToRockets: () => Console.WriteLine("→ Rockets"),
    navigateToMap: () => Console.WriteLine("→ Map")
);

homeVm.NavigateToLaunchesCommand.Execute(null);
// Output: → Launches
```

---

## 3. Modelos de Datos

### 3.1 LaunchModel

**Ubicación:** `Models/LaunchModel.cs`

```csharp
public class LaunchModel
{
    public string Id { get; set; }              // ID único (UUID)
    public string Name { get; set; }            // Nombre del lanzamiento
    public DateTime DateUtc { get; set; }      // Fecha/hora UTC
    public string RocketId { get; set; }       // ID del cohete usado
    public string LaunchpadId { get; set; }    // ID del sitio
    public bool? Success { get; set; }         // null=pendiente, true=éxito, false=fallo
    public string Details { get; set; }        // Descripción/notas
    public bool Upcoming { get; set; }         // ¿Es un lanzamiento futuro?
    
    // Propiedades calculadas (read-only)
    public string Status { get; }              // "Próximo" / "Exitoso" / "Fallido"
    public string DateFormatted { get; }       // "dd/MM/yyyy HH:mm"
}
```

---

### 3.2 RocketModel

**Ubicación:** `Models/RocketModel.cs`

```csharp
public class RocketModel
{
    public string Id { get; set; }
    public string Name { get; set; }                // Ej: "Falcon 9"
    public string Type { get; set; }
    public bool Active { get; set; }                // ¿Está activo?
    public int Stages { get; set; }                 // Número de etapas
    public int Boosters { get; set; }
    public long CostPerLaunch { get; set; }        // Costo en USD
    public double SuccessRatePct { get; set; }    // % éxito
    public string FirstFlight { get; set; }        // Fecha primer vuelo
    public string Country { get; set; }             // País fabricante
    public string Company { get; set; }             // Compañía (SpaceX)
    public double? HeightMeters { get; set; }
    public double? HeightFeet { get; set; }
    public double? DiameterMeters { get; set; }
    public double? DiameterFeet { get; set; }
    public long MassKg { get; set; }
    public long MassLb { get; set; }
    public int EnginesNumber { get; set; }
    public string EnginesType { get; set; }
    public string Description { get; set; }
    
    // Propiedades calculadas (read-only)
    public string Status { get; }                  // "Activo" / "Inactivo"
    public string HeightFormatted { get; }        // "70.0m (229.6ft)"
    public string CostFormatted { get; }          // "$62,000,000"
}
```

---

### 3.3 LaunchpadModel

**Ubicación:** `Models/LaunchpadModel.cs`

```csharp
public class LaunchpadModel
{
    public string Id { get; set; }                 // ID único (UUID)
    public string Name { get; set; }               // Ej: "Kennedy Space Center LC-39A"
    public double Latitude { get; set; }          // Para mapas
    public double Longitude { get; set; }          // Para mapas
    public string Region { get; set; }             // Ej: "Florida"
    public string FullName { get; set; }
    public int LaunchAttempts { get; set; }       // Total intentos
    public int LaunchSuccesses { get; set; }      // Intentos exitosos
}
```

---

### 3.4 MapPointModel

**Ubicación:** `Models/MapPointModel.cs`

```csharp
public class MapPointModel
{
    public string Id { get; set; }                 // ID único
    public string Name { get; set; }               // Nombre del sitio
    public double Latitude { get; set; }          // Coordenada geográfica
    public double Longitude { get; set; }          // Coordenada geográfica
    public int LaunchCount { get; set; }          // Total lanzamientos
    public int SuccessCount { get; set; }         // Lanzamientos exitosos
    
    // Propiedad calculada (read-only)
    public string Info { get; }                    // "{Name}\n{LaunchCount} lanzamientos\n{SuccessCount} exitosos"
}
```

---

## 4. Data Transfer Objects (DTOs)

**Ubicación:** `Services/DTOs/`

Los DTOs mapean 1:1 con la respuesta JSON de SpaceX API. Se deserializan automáticamente y luego se mapean a Models.

### Flujo: API JSON → DTO → Model

```
SpaceX API JSON response
    ↓
JsonSerializer.Deserialize<RocketDto>(json)
    ↓
RocketDto (properties coinciden con JSON)
    ↓
Mapeo a RocketModel en SpaceXApiService.MapToRocketModel()
    ↓
RocketModel (datos puros sin JSON)
```

**Ventaja:** Separation of concerns. Models no conocen JSON, DTOs no son negocio.

---

## 5. Excepciones

### InvalidOperationException

Se lanza cuando hay error comunicando con SpaceX API.

**Contexto:**
```csharp
catch (Exception ex)
{
    throw new InvalidOperationException(
        "Error al obtener [recurso] de la API", 
        ex);
}
```

**Manejo en ViewModels:**
```csharp
catch (Exception ex)
{
    ErrorMessage = $"Error: {ex.Message}";  // Mostrado en UI
}
```

---

## 6. Convertidores XAML

### StringToVisibilityConverter

**Entrada:** string (ErrorMessage)

**Lógica:**
```csharp
if (string.IsNullOrWhiteSpace(value))
    return Visibility.Collapsed;  // Oculta si vacío
else
    return Visibility.Visible;    // Muestra si tiene contenido
```

**Uso en XAML:**
```xaml
<Border Visibility="{Binding ErrorMessage,
                    Converter={StaticResource StringToVisibilityConverter}}">
    <TextBlock Text="{Binding ErrorMessage}" />
</Border>
```

### BoolToVisibilityConverter

**Entrada:** bool (IsLoading)

**Lógica:**
```csharp
return (bool)value ? Visibility.Visible : Visibility.Collapsed;
```

**Uso en XAML:**
```xaml
<Border Visibility="{Binding IsLoading,
                    Converter={StaticResource BoolToVisibilityConverter}}">
    <ProgressBar IsIndeterminate="True" />
</Border>
```

---

## 7. Patrones de Uso Comunes

### Patrón 1: Cargar y Filtrar

```csharp
// En ViewModel
[RelayCommand]
public async Task LoadData()
{
    IsLoading = true;
    try
    {
        var data = await _apiService.GetXxxAsync();
        Collection = new ObservableCollection(data);
        ApplyFilters();  // Filtro automático
    }
    catch (Exception ex)
    {
        ErrorMessage = ex.Message;
    }
    finally
    {
        IsLoading = false;
    }
}

// En XAML
<DataGrid ItemsSource="{Binding FilteredCollection}" />
<TextBox Text="{Binding SearchText, UpdateSourceTrigger=PropertyChanged}" />
```

### Patrón 2: Navegación Reactiva

```csharp
// En MainWindow.xaml
<ContentControl Content="{Binding CurrentViewModel}">
    <ContentControl.Resources>
        <DataTemplate DataType="{x:Type vm:MyViewModel}">
            <views:MyView />
        </DataTemplate>
    </ContentControl.Resources>
</ContentControl>

// En ViewModel
[ObservableProperty]
private ObservableObject currentViewModel;

[RelayCommand]
public void NavigateTo()
{
    CurrentViewModel = _nextViewModel;  // PropertyChanged dispara automáticamente
}
```

### Patrón 3: Post-Construction Injection para Evitar Circular

```csharp
// En MainViewModel constructor
_launchesViewModel.SetNavigateToHome(NavigateToHome);

// En LaunchesViewModel
private Action _navigateToHome;
public void SetNavigateToHome(Action action) => _navigateToHome = action;

[RelayCommand]
public void GoHome() => _navigateToHome?.Invoke();
```

---