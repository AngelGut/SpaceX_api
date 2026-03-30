# Documentación Técnica

**EspaceX_api - Arquitectura, Stack y Componentes**

---

## 1. Stack Tecnológico

| Componente | Versión | Propósito |
|-----------|---------|----------|
| **Lenguaje** | C# 12 | Lenguaje de programación principal |
| **Framework** | .NET 8.0 | Runtime y librerías base |
| **UI Framework** | WPF (Windows Presentation Foundation) | Interfaz gráfica de escritorio |
| **Patrón MVVM** | CommunityToolkit.MVVM v8.2.2 | Propiedades observables y comandos |
| **Inyección DI** | Microsoft.Extensions.DependencyInjection | Contenedor de dependencias |
| **HTTP Client** | System.Net.Http.HttpClient | Llamadas a API |
| **Serialización JSON** | System.Text.Json | Deserialización de respuestas API |
| **Control de versiones** | Git / GitHub | Repositorio con Gitflow |
| **API Externa** | SpaceX API v4 | Fuente de datos |

---

## 2. Arquitectura MVVM

```
┌─────────────────────────────────────────┐
│    PRESENTATION LAYER (Views)            │
│ HomeView, LaunchesView, RocketsView,    │
│ MapView, MainWindow                     │
└──────────────────┬──────────────────────┘
                   │ Data Binding
┌──────────────────▼──────────────────────┐
│  VIEWMODEL LAYER (Lógica Presentación)  │
│ HomeViewModel, LaunchesViewModel,       │
│ RocketsViewModel, MapViewModel,         │
│ MainViewModel                           │
└──────────────────┬──────────────────────┘
                   │ Métodos/Propiedades
┌──────────────────▼──────────────────────┐
│    SERVICE LAYER (Acceso a Datos)       │
│ ISpaceXApiService                       │
│ SpaceXApiService (impl + Caché)         │
└──────────────────┬──────────────────────┘
                   │ Mapeo DTO → Model
┌──────────────────▼──────────────────────┐
│    MODEL LAYER (Entidades)              │
│ LaunchModel, RocketModel,               │
│ LaunchpadModel, MapPointModel           │
└──────────────────┬──────────────────────┘
                   │ HTTP Request
┌──────────────────▼──────────────────────┐
│  EXTERNAL API LAYER                     │
│ SpaceX API v4 (api.spacexdata.com)     │
└─────────────────────────────────────────┘
```

---

## 3. Estructura del Proyecto

```
EspaceX_api/
│
├── Models/                          # Entidades de dominio (sin JSON)
│   ├── LaunchModel.cs               # Lanzamiento
│   ├── RocketModel.cs               # Cohete
│   ├── LaunchpadModel.cs            # Sitio de lanzamiento
│   └── MapPointModel.cs             # Punto en mapa
│
├── Services/                        # Capa de acceso a datos
│   ├── ISpaceXApiService.cs         # Interfaz
│   ├── SpaceXApiService.cs          # Implementación + Caché
│   └── DTOs/                        # Mapeo 1:1 con JSON
│       ├── LaunchDto.cs
│       ├── RocketDto.cs
│       └── LaunchpadDto.cs
│
├── ViewModels/                      # Lógica de presentación
│   ├── MainViewModel.cs             # Coordinador de navegación
│   ├── HomeViewModel.cs             # Dashboard
│   ├── LaunchesViewModel.cs         # Lanzamientos
│   ├── RocketsViewModel.cs          # Cohetes
│   └── MapViewModel.cs              # Mapa interactivo
│
├── Views/                           # Interfaz de usuario (XAML)
│   ├── MainWindow.xaml              # Ventana principal
│   ├── MainWindow.xaml.cs           # Code-behind (mínimo)
│   ├── HomeView.xaml                # Home
│   ├── HomeView.xaml.cs
│   ├── LaunchesView.xaml            # Lanzamientos
│   ├── LaunchesView.xaml.cs
│   ├── RocketsView.xaml             # Cohetes
│   ├── RocketsView.xaml.cs
│   ├── MapView.xaml                 # Mapa
│   └── MapView.xaml.cs
│
├── App.xaml                         # Recursos globales, estilos
├── App.xaml.cs                      # Configuración DI, convertidores
├── EspaceX_api.csproj               # Definición del proyecto
│
└── Docs/                            # Documentación (este contenido)
    ├── DOCUMENTACION_REQUISITOS.md
    ├── DOCUMENTACION_TECNICA.md
    ├── DOCUMENTACION_API.md
    └── DOCUMENTACION_DECISIONES.md
```

---

## 4. Componentes Principales

### 4.1 Interfaz de Inyección de Dependencias (ISpaceXApiService)

**Ubicación:** `Services/ISpaceXApiService.cs`

**Responsabilidad:** Define el contrato para acceso a datos de SpaceX.

**Métodos públicos:**

```csharp
Task<List<LaunchModel>> GetLaunchesAsync()           // Todos los lanzamientos
Task<List<RocketModel>> GetRocketsAsync()            // Todos los cohetes
Task<LaunchpadModel> GetLaunchpadAsync(string id)    // Sitio específico por ID
Task<RocketModel> GetRocketAsync(string rocketId)    // Cohete específico por ID
void ClearCache()                                    // Limpia caché
bool IsCacheExpired()                                // Verifica expiración caché
```

### 4.2 Servicio de Datos (SpaceXApiService)

**Ubicación:** `Services/SpaceXApiService.cs`

**Responsabilidad:** 
- Consumir SpaceX API v4
- Deserializar JSON a DTOs
- Mapear DTOs a Models
- Implementar caché en memoria

**Características:**

| Característica | Detalles |
|---|---|
| **Base URL** | https://api.spacexdata.com/v4 |
| **Endpoints usados** | /launches, /rockets, /launchpads |
| **Caché lanzamientos** | List<LaunchModel> con expiración 1 hora |
| **Caché cohetes** | List<RocketModel> con expiración 1 hora |
| **Caché launchpads** | Dictionary<string, LaunchpadModel> (sesión completa) |
| **Serialización JSON** | PropertyNameCaseInsensitive = true |

### 4.3 Modelos de Datos (Models)

**LaunchModel**
- Propiedades: Id, Name, DateUtc, RocketId, LaunchpadId, Success, Details, Upcoming
- Propiedades calculadas: Status (Exitoso/Fallido/Próximo), DateFormatted

**RocketModel**
- Propiedades: Id, Name, Type, Active, Stages, Boosters, CostPerLaunch, SuccessRatePct, FirstFlight, Country, Company, HeightMeters, HeightFeet, DiameterMeters, DiameterFeet, MassKg, MassLb, EnginesNumber, EnginesType, Description
- Propiedades calculadas: Status (Activo/Inactivo), HeightFormatted, CostFormatted

**LaunchpadModel**
- Propiedades: Id, Name, Latitude, Longitude, Region, FullName, LaunchAttempts, LaunchSuccesses

**MapPointModel**
- Propiedades: Id, Name, Latitude, Longitude, LaunchCount, SuccessCount
- Propiedades calculadas: Info (string resumen)

### 4.4 ViewModels

#### MainViewModel
- **Responsabilidad:** Coordinación global y navegación
- **Propiedades:** CurrentViewModel (ObservableProperty<ObservableObject>)
- **Comandos:** NavigateToHome, NavigateToLaunches, NavigateToRockets, NavigateToMap, Exit
- **Patrones:** Inyecta lambdas de navegación en VMs secundarios para evitar dependencia circular

#### HomeViewModel
- **Responsabilidad:** Mostrar dashboard interactivo
- **Parámetros constructor:** navigateToLaunches, navigateToRockets, navigateToMap (Action)
- **Comandos:** NavigateToLaunches, NavigateToRockets, NavigateToMap
- **Particularidad:** Construido por MainViewModel (no registrado en DI)

#### LaunchesViewModel
- **Responsabilidad:** Gestionar estado y lógica de lanzamientos
- **Propiedades:** Launches, FilteredLaunches, SearchText, StatusFilter, IsLoading, ErrorMessage
- **Comandos:** LoadLaunches, Refresh, GoHome
- **Observadores:** OnSearchTextChanged, OnStatusFilterChanged (disparan ApplyFilters)
- **Lógica:** Filtrado LINQ, caché 1 hora

#### RocketsViewModel
- **Responsabilidad:** Gestionar estado y lógica de cohetes
- **Propiedades:** Rockets, SelectedRocket, IsLoading, ErrorMessage
- **Comandos:** LoadRockets, Refresh, GoHome
- **Lógica:** Carga datos, permite selección

#### MapViewModel
- **Responsabilidad:** Gestionar mapa interactivo y sitios
- **Propiedades:** LaunchSites, SelectedSite, SelectedSiteLaunches, ZoomLevel, PanX, PanY, IsLoading, ErrorMessage
- **Comandos:** LoadLaunchSites, ZoomIn, ZoomOut, ResetView, GoHome
- **Métodos públicos:** GeographicToScreenCoordinates, BeginDrag, EndDrag, UpdateDrag
- **Lógica:** Carga sitios llamando GetLaunchpadAsync por ID, proyección Mercator, zoom/pan

### 4.5 Vistas (Views)

#### MainWindow.xaml
- **Responsabilidad:** Shell principal
- **Contenido:** ContentControl con DataTemplates que mapean cada ViewModel a su View
- **DataTemplates:** HomeViewModel→HomeView, LaunchesViewModel→LaunchesView, etc.
- **Code-behind:** Mínimo - solo asigna DataContext a MainViewModel

#### HomeView
- **Responsabilidad:** Mostrar dashboard visual
- **Elemento visual:** Canvas con estrellas + animaciones + tarjetas
- **Estilos:** Fondo #020810, tarjetas con colores: cyan (lanzamientos), naranja (cohetes), verde (mapa)
- **Code-behind:** Mínimo - solo InitializeComponent
- **Interacción:** Clic en tarjeta via MouseBinding (no Button) → comando ViewModel

#### LaunchesView
- **Responsabilidad:** Mostrar y filtrar lanzamientos
- **Contenidos:** Toolbar (búsqueda + filtro), DataGrid, mensajes error
- **DataGrid:** Columnas Name, DateFormatted, Status (badge color), Details
- **Bindings:** ItemsSource={Binding FilteredLaunches}
- **Code-behind:** Mínimo - solo InitializeComponent

#### RocketsView
- **Responsabilidad:** Mostrar especificaciones de cohetes
- **Contenidos:** Toolbar (cargar/actualizar), DataGrid
- **DataGrid:** Columnas Name, Height, Mass, Cost, SuccessRate, Engines
- **Code-behind:** Mínimo

#### MapView
- **Responsabilidad:** Renderizar mapa y capturar interacciones
- **Canvas:** Dibuja continentes, grilla, puntos
- **Sidebar:** ListBox de sitios + DataGrid de lanzamientos del sitio
- **Code-behind:** DrawMap, DrawContinents, DrawGrid, DrawLaunchSite, eventos mouse
- **Conversores:** BoolToVisibilityConverter para overlay de carga
- **Particularidad:** Code-behind contiene dibujo (presentación), lógica en ViewModel

### 4.6 Configuración Global (App.xaml.cs)

**Responsabilidades:**
- Configurar contenedor DI (ServiceCollection)
- Registrar servicios (ISpaceXApiService → SpaceXApiService)
- Registrar ViewModels (Singleton para persistencia de estado)
- Registrar Convertidores (StringToVisibilityConverter, BoolToVisibilityConverter)
- Crear MainWindow y mostrarla

**Registros en DI:**
```
services.AddSingleton<ISpaceXApiService, SpaceXApiService>()
services.AddSingleton<LaunchesViewModel>()
services.AddSingleton<RocketsViewModel>()
services.AddSingleton<MapViewModel>()
services.AddSingleton<MainViewModel>()
services.AddSingleton<MainWindow>()
// HomeViewModel NO se registra (construido por MainViewModel)
```

---

## 5. Flujo de Datos

### Flujo 1: Cargar Lanzamientos

```
Usuario hace clic botón "Cargar"
    ↓
XAML Command binding → LaunchesViewModel.LoadLaunchesCommand
    ↓
LoadLaunches() async Task se ejecuta
    ↓
IsLoading = true (notifica UI que muestre indicador)
    ↓
await _apiService.GetLaunchesAsync()
    ↓
SpaceXApiService verifica caché
    ├─ Si válido (< 1 hora) → devuelve caché
    └─ Si expirado → HTTP GET /launches → JsonSerializer.Deserialize → mapea DTOs a Models → cachea resultado
    ↓
Launches = new ObservableCollection(resultado)
    ↓
PropertyChanged dispara
    ↓
ApplyFilters() se ejecuta automáticamente (sin usuario hacer nada)
    ↓
FilteredLaunches = resultado filtrado
    ↓
PropertyChanged dispara
    ↓
XAML DataGrid observa cambio en FilteredLaunches
    ↓
UI se redibuja con nuevos datos
    ↓
IsLoading = false → overlay desaparece
    ↓
Usuario ve tabla cargada y filtrada
```

### Flujo 2: Filtrar por Búsqueda

```
Usuario escribe en TextBox de búsqueda
    ↓
SearchText binding UpdateSourceTrigger=PropertyChanged
    ↓
OnSearchTextChanged(value) observador automático
    ↓
ApplyFilters() LINQ: where Name.Contains(SearchText, OrdinalIgnoreCase)
    ↓
FilteredLaunches = ObservableCollection(filtered)
    ↓
PropertyChanged dispara
    ↓
DataGrid observa y redibuja instantáneamente
    ↓
Usuario ve resultados filtrados en tiempo real
```

### Flujo 3: Navegar a Mapa

```
Usuario hace clic en tarjeta "Mapa" del Home
    ↓
HomeViewModel.NavigateToMapCommand
    ↓
MainViewModel.NavigateToMap() se ejecuta
    ↓
MainViewModel.CurrentViewModel = _mapViewModel
    ↓
PropertyChanged dispara
    ↓
ContentControl observa cambio en CurrentViewModel
    ↓
Busca DataTemplate para MapViewModel en su Resources
    ↓
Instancia <views:MapView />
    ↓
Automáticamente asigna DataContext = MapViewModel
    ↓
MapView se renderiza
    ↓
MapView.xaml.cs OnLoaded(): subscribes a PropertyChanged del ViewModel
    ↓
MapView.xaml.cs Loaded event: llamar DrawMap() → dibuja continentes + grilla (vacío de puntos)
    ↓
Usuario ve mapa sin puntos (correcto comportamiento)
    ↓
Usuario presiona "Cargar Sitios"
    ↓
LoadLaunchSitesCommand
    ↓
LoadLaunchSites() async: 
  1. Obtiene todos los lanzamientos (GetLaunchesAsync)
  2. Extrae IDs únicos de launchpads
  3. Itera llamando GetLaunchpadAsync(id) por cada uno
  4. Mapea a MapPointModel
  5. Asigna LaunchSites = new ObservableCollection(resultado)
    ↓
PropertyChanged dispara (en LaunchSites)
    ↓
MapView.xaml.cs PropertyChanged listener (especialmente LaunchSites)
    ↓
DrawMap() se ejecuta (pero aún sin SelectedSite, solo dibuja continentes)
    ↓
Usuario selecciona sitio en ListBox
    ↓
SelectedSite asignado a MapPointModel seleccionado
    ↓
PropertyChanged dispara (en SelectedSite)
    ↓
OnSelectedSiteChanged(value) observador ViewModel: carga últimos 20 lanzamientos del sitio
    ↓
SelectedSiteLaunches se actualiza
    ↓
PropertyChanged dispara (en SelectedSite también para dibujo)
    ↓
MapView.xaml.cs DrawMap() se ejecuta
    ↓
DrawMap() verifica: if (ViewModel?.SelectedSite == null) return
    ↓
Dibuja punto rojo + halo para sitio seleccionado
    ↓
Usuario ve punto en mapa + tabla de lanzamientos en sidebar
```

---

## 6. Patrón MVVM

| Aspecto | Implementación |
|--------|---|
| **Separación** | View no conoce ViewModel, ViewModel no conoce View |
| **Data Binding** | XAML bindings a propiedades ObservableProperty |
| **Observables** | CommunityToolkit.MVVM [ObservableProperty] genera PropertyChanged |
| **Comandos** | [RelayCommand] genera ICommand sin boilerplate |
| **No Code-Behind** | Views solo InitializeComponent (excepto MapView dibuja) |
| **Navegación** | ContentControl + DataTemplate (reactiva, sin código) |

---

## 7. Patrón de Inyección de Dependencias

| Patrón | Descripción |
|--------|-----------|
| **Constructor Injection** | LaunchesViewModel(ISpaceXApiService apiService) |
| **Post-Construction Injection** | LaunchesViewModel.SetNavigateToHome(Action) para evitar circular |
| **ServiceCollection** | Registra Singleton para mantener estado |
| **Ciclo Singleton** | Servicios + ViewModels (estado persistente entre vistas) |

---

## 8. Convertidores XAML Personalizados

### StringToVisibilityConverter
- **Entrada:** string (ErrorMessage)
- **Salida:** Visibility (Visible si tiene contenido, Collapsed si vacío)
- **Uso:** Mostrar/ocultar mensaje error automáticamente

### BoolToVisibilityConverter
- **Entrada:** bool (IsLoading)
- **Salida:** Visibility (Visible si true, Collapsed si false)
- **Uso:** Mostrar/ocultar overlay de carga automáticamente

---

## 9. Configuración de Deserialización JSON

```csharp
var jsonOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,  // Tolera casos distintos: launchpad vs Launchpad
    WriteIndented = false
};
```

**Razón:** SpaceX API devuelve propiedades en snake_case (launch_date), DTOs en PascalCase (LaunchDate).

---

## 10. Caché en Memoria

### Implementación

```csharp
private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(1);
private List<LaunchModel> _launchesCache = new();
private DateTime _launchesCacheTime = DateTime.MinValue;

public async Task<List<LaunchModel>> GetLaunchesAsync()
{
    if (IsCacheValid(_launchesCacheTime) && _launchesCache.Any())
        return _launchesCache;  // Devuelve caché
    
    // Consulta API
    var response = await _httpClient.GetAsync($"{BaseUrl}/launches");
    // Deserializa y mapea
    _launchesCache = MapToModels(dtos);
    _launchesCacheTime = DateTime.UtcNow;
    return _launchesCache;
}

private bool IsCacheValid(DateTime cacheTime) =>
    DateTime.UtcNow - cacheTime < _cacheExpiration;
```

### Ventajas
- **Performance:** Datos disponibles instantáneamente
- **Ancho de banda:** Reduce llamadas innecesarias a API
- **UX:** Respuesta rápida
- **Tolerancia:** App funciona con caché si internet cae

### Control Manual
- `ClearCache()` limpia todo
- `IsCacheExpired()` verifica estado
- Botón "Actualizar" en UI llama ClearCache + recarga

---

## 11. Dependencia Circular Resuelta

### Problema Original
```
MainViewModel necesita HomeViewModel
HomeViewModel necesita MainViewModel para navegar
→ StackOverflowException en DI
```

### Solución: Inyección de Acciones

```csharp
// HomeViewModel recibe lambdas, no MainViewModel
public HomeViewModel(Action navigateToLaunches, 
                     Action navigateToRockets, 
                     Action navigateToMap)

// MainViewModel las provee
_homeViewModel = new HomeViewModel(
    navigateToLaunches: NavigateToLaunches,
    navigateToRockets: NavigateToRockets,
    navigateToMap: NavigateToMap
);

// ViewModels secundarios reciben en constructor
_launchesViewModel.SetNavigateToHome(NavigateToHome);
```

**Patrón:** Method Injection / Setter Injection (válido cuando constructor injection no es posible)

---

## 12. Renderizado del Mapa (Caso Especial)

**¿Por qué code-behind en MapView?**

WPF no tiene binding para dibujo dinámico de polígonos. Solución estándar de la industria:

```csharp
// MapView.xaml.cs (presentación pura)
private void DrawMap() 
{
    MapCanvas.Children.Clear();
    DrawContinents(width, height);
    DrawGrid(width, height);
    if (ViewModel?.SelectedSite != null) DrawLaunchSite(...);
}

// MapViewModel (lógica pura)
public (double X, double Y) GeographicToScreenCoordinates(double lat, double lon, double w, double h)
{
    // Cálculo matemático (Mercator + zoom + pan)
    // No toca Canvas, Polygons ni UI
}
```

**Cumple MVVM:** Lógica en ViewModel, presentación en View.

---
