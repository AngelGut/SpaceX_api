# 🏗️ Arquitectura de EspaceX_api

## Tabla de Contenidos

1. [Visión General](#visión-general)
2. [Principios SOLID](#principios-solid)
3. [Inyección de Dependencias](#inyección-de-dependencias)
4. [Patrón MVVM](#patrón-mvvm)
5. [Flujo de Datos](#flujo-de-datos)
6. [Estructura de Carpetas](#estructura-de-carpetas)

---

## Visión General

EspaceX_api sigue una **arquitectura en capas** con separación clara de responsabilidades:

```
┌─────────────────────────────────────┐
│         PRESENTATION LAYER          │
│  Views (XAML) + ViewModels          │
└──────────────┬──────────────────────┘
               │ binding
┌──────────────▼──────────────────────┐
│      VIEWMODELS LAYER               │
│ Lógica de negocio, estado, UI       │
└──────────────┬──────────────────────┘
               │ inyección
┌──────────────▼──────────────────────┐
│        SERVICE LAYER                │
│ ISpaceXApiService (Interfaz)        │
│ SpaceXApiService (Implementación)   │
└──────────────┬──────────────────────┘
               │ mapeo
┌──────────────▼──────────────────────┐
│        DATA LAYER                   │
│ DTOs (deserialización)              │
│ Models (datos puros)                │
└──────────────┬──────────────────────┘
               │ HTTP
┌──────────────▼──────────────────────┐
│        DATA SOURCE LAYER            │
│  SpaceX API v4 (externa)            │
└─────────────────────────────────────┘
```

---

## Principios SOLID

### 1️⃣ **Single Responsibility Principle (SRP)**

Cada clase tiene **una única razón para cambiar**.

**✅ Bien:**
```csharp
// SpaceXApiService - Responsabilidad ÚNICA: comunicar con API + mapear
public class SpaceXApiService : ISpaceXApiService
{
    public async Task<List<LaunchModel>> GetLaunchesAsync() { ... }
}

// LaunchesViewModel - Responsabilidad ÚNICA: gestionar estado de lanzamientos
public class LaunchesViewModel : ObservableObject
{
    public async Task LoadLaunches() { ... }
}
```

---

### 2️⃣ **Open/Closed Principle (OCP)**

Las clases deben estar **abiertas a extensión, cerradas a modificación**.

**✅ Bien:**
```csharp
// Interfaz que define el contrato
public interface ISpaceXApiService
{
    Task<List<LaunchModel>> GetLaunchesAsync();
}

// Implementación actual
public class SpaceXApiService : ISpaceXApiService { ... }

// Puedo crear OTRA implementación sin tocar código existente
public class MockSpaceXApiService : ISpaceXApiService { ... }

// En App.xaml.cs cambio la implementación fácilmente:
services.AddSingleton<ISpaceXApiService, SpaceXApiService>();
// ó
services.AddSingleton<ISpaceXApiService, MockSpaceXApiService>();
```

---

### 3️⃣ **Liskov Substitution Principle (LSP)**

Cualquier implementación de `ISpaceXApiService` debe poder **reemplazar a otra sin romper el código**.

**✅ Bien:**
```csharp
ISpaceXApiService service1 = new SpaceXApiService();
ISpaceXApiService service2 = new MockSpaceXApiService();

var launches = await service1.GetLaunchesAsync(); // Funciona igual
launches = await service2.GetLaunchesAsync();     // Mismo tipo de resultado
```

---

### 4️⃣ **Interface Segregation Principle (ISP)**

Las interfaces deben ser **pequeñas y específicas**, no genéricas.

**✅ Bien:**
```csharp
// Interfaz pequeña y específica
public interface ISpaceXApiService
{
    Task<List<LaunchModel>> GetLaunchesAsync();
    Task<List<RocketModel>> GetRocketsAsync();
    Task<LaunchpadModel> GetLaunchpadAsync(string id);
}
```

---

### 5️⃣ **Dependency Inversion Principle (DIP)**

Los módulos deben **depender de abstracciones (interfaces), no de implementaciones concretas**.

**✅ Bien:**
```csharp
// ViewModel depende de INTERFAZ, no de clase concreta
public class LaunchesViewModel : ObservableObject
{
    private readonly ISpaceXApiService _apiService; // INTERFAZ

    public LaunchesViewModel(ISpaceXApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task LoadLaunches()
    {
        var launches = await _apiService.GetLaunchesAsync();
    }
}

// En App.xaml.cs: el contenedor DI decide qué implementación usar
services.AddSingleton<ISpaceXApiService, SpaceXApiService>();
```

---

## Inyección de Dependencias

### ¿Qué es DI?

Es un patrón donde las **dependencias se "inyectan" desde afuera** en lugar de crearlas adentro.

### Configuración en App.xaml.cs

```csharp
private void ConfigureServices(IServiceCollection services)
{
    // 1. Registrar el servicio (interfaz → implementación)
    services.AddSingleton<ISpaceXApiService, SpaceXApiService>();
    // ↑ Cada vez que alguien pida ISpaceXApiService, obtiene SpaceXApiService

    // 2. Registrar ViewModels (cada uno depende de ISpaceXApiService)
    services.AddSingleton<HomeViewModel>();
    services.AddSingleton<LaunchesViewModel>();
    services.AddSingleton<RocketsViewModel>();
    services.AddSingleton<MapViewModel>();
    
    // 3. Registrar MainViewModel (depende de otros ViewModels)
    services.AddSingleton<MainViewModel>();
    
    // 4. Registrar MainWindow
    services.AddSingleton<MainWindow>();
}

// Al iniciar
protected override void OnStartup(StartupEventArgs e)
{
    var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
    // El contenedor DI automáticamente:
    // 1. Crea MainViewModel
    // 2. Ve que necesita HomeViewModel, LaunchesViewModel, etc.
    // 3. Los crea y los inyecta
    // 4. Todo está conectado automáticamente
    mainWindow.Show();
}
```

### Ventajas de DI

| Ventaja | Explicación |
|---------|-------------|
| **Testabilidad** | Puedo inyectar mock para testing |
| **Mantenibilidad** | Cambiar implementación sin modificar consumidores |
| **Flexibilidad** | Cambiar en un lugar (App.xaml.cs), afecta todo |
| **Desacoplamiento** | Menos dependencias entre clases |

---

## Patrón MVVM

### Estructura MVVM con MVVM Toolkit

```
View (XAML)
    ↕ Data Binding
ViewModel (Lógica)
    ↕ Método llamada
Service (Datos)
```

### Ejemplo: LaunchesViewModel

```csharp
public partial class LaunchesViewModel : ObservableObject
{
    // Propiedad observable (MVVM Toolkit)
    [ObservableProperty]
    private ObservableCollection<LaunchModel> launches = new();
    // ↑ Automáticamente genera PropertyChanged, backing field, etc.

    // Comando observable (MVVM Toolkit)
    [RelayCommand]
    public async Task LoadLaunches()
    {
        // Se ejecuta cuando el botón en XAML hace clic
        var launches = await _apiService.GetLaunchesAsync();
        Launches = new ObservableCollection<LaunchModel>(launches);
    }

    // Observer automático: se ejecuta cuando cambia SearchText
    partial void OnSearchTextChanged(string value)
    {
        ApplyFilters();
    }
}
```

### Binding en XAML

```xaml
<!-- Binding a propiedad observable -->
<DataGrid ItemsSource="{Binding FilteredLaunches}" />

<!-- Binding a comando -->
<Button Command="{Binding LoadLaunchesCommand}" />

<!-- Binding bidireccional con actualización en tiempo real -->
<TextBox Text="{Binding SearchText, UpdateSourceTrigger=PropertyChanged}" />
```

---

## Flujo de Datos

### 1. Usuario hace clic en "Cargar Lanzamientos"

```
XAML Button
    ↓ command binding
LaunchesViewModel.LoadLaunchesCommand
    ↓ async call
ISpaceXApiService.GetLaunchesAsync()
    ↓ HTTP request
SpaceX API v4
    ↓ JSON response
JsonSerializer.Deserialize<List<LaunchDto>>()
    ↓ mapeo
DTO → LaunchModel
    ↓ asigna
LaunchesViewModel.Launches = new ObservableCollection(result)
    ↓ PropertyChanged notificación
XAML DataGrid
    ↓ actualiza UI
Usuario ve datos
```

### 2. Usuario busca en el TextBox

```
TextBox (SearchText binding)
    ↓ PropertyChanged
OnSearchTextChanged() método
    ↓ llama
ApplyFilters()
    ↓ LINQ filtering
FilteredLaunches = resultado filtrado
    ↓ PropertyChanged notificación
XAML DataGrid
    ↓ actualiza UI
Usuario ve resultados filtrados
```

---

## Estructura de Carpetas

```
EspaceX_api/
│
├── Models/
│   ├── LaunchModel.cs        # Datos puros (sin JSON)
│   ├── RocketModel.cs        # Datos puros (sin JSON)
│   └── LaunchpadModel.cs     # Datos puros (sin JSON)
│
├── Services/
│   ├── ISpaceXApiService.cs  # Contrato (Interface)
│   ├── SpaceXApiService.cs   # Implementación (API + mapeo)
│   └── DTOs/
│       ├── LaunchDto.cs      # Mapeo 1:1 con JSON
│       ├── RocketDto.cs      # Mapeo 1:1 con JSON
│       └── LaunchpadDto.cs   # Mapeo 1:1 con JSON
│
├── ViewModels/
│   ├── MainViewModel.cs      # Coordina navegación
│   ├── HomeViewModel.cs      # Lógica de inicio (PERSONA 1)
│   ├── LaunchesViewModel.cs  # Lógica de lanzamientos (PERSONA 2)
│   ├── RocketsViewModel.cs   # Lógica de cohetes (PERSONA 3)
│   └── MapViewModel.cs       # Lógica del mapa (PERSONA 4)
│
├── Views/
│   ├── MainWindow.xaml       # Shell principal (casi no cambia)
│   ├── MainWindow.xaml.cs    # Code-behind mínimo
│   ├── HomeView.xaml         # Diseño home (PERSONA 1)
│   ├── HomeView.xaml.cs      # Code-behind home (PERSONA 1)
│   ├── LaunchesView.xaml     # Diseño lanzamientos (PERSONA 2)
│   ├── LaunchesView.xaml.cs  # Code-behind (PERSONA 2)
│   ├── RocketsView.xaml      # Diseño cohetes (PERSONA 3)
│   ├── RocketsView.xaml.cs   # Code-behind (PERSONA 3)
│   ├── MapView.xaml          # Diseño mapa (PERSONA 4)
│   └── MapView.xaml.cs       # Code-behind (PERSONA 4)
│
├── App.xaml                  # Estilos globales
├── App.xaml.cs               # DI + Convertidor
├── EspaceX_api.csproj        # Configuración proyecto
│
└── docs/
    ├── ARQUITECTURA.md       # Este archivo
    ├── GUIA_DESARROLLO.md    # Cómo extender
    └── API_REFERENCE.md      # Referencia de métodos
```

---

## Caché de Datos

El servicio implementa caché **en memoria** con expiración automática:

```csharp
private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(1);

public async Task<List<LaunchModel>> GetLaunchesAsync()
{
    // Si el caché es válido y tiene datos, devuelve cached
    if (IsCacheValid(_launchesCacheTime) && _launchesCache.Any())
    {
        return _launchesCache;
    }

    // Si no, hace request y cachea resultado
    var response = await _httpClient.GetAsync($"{BaseUrl}/launches");
    var dtos = JsonSerializer.Deserialize<List<LaunchDto>>(json);
    
    // Mapeo: DTO → Model
    _launchesCache = dtos.Select(MapToLaunchModel).ToList();
    _launchesCacheTime = DateTime.UtcNow;
    
    return _launchesCache;
}
```

**Ventajas:**
- No golpea API innecesariamente
- UX rápida (datos locales)
- Reduce ancho de banda

---

## Conclusión

EspaceX_api demuestra:

✅ **Separación clara de responsabilidades** (SRP)
✅ **Extensibilidad sin modificación** (OCP)
✅ **Interfaces bien definidas** (ISP)
✅ **Inyección de dependencias** (DIP)
✅ **Patrón MVVM profesional** con MVVM Toolkit
✅ **Caché inteligente** de datos
✅ **Código testeable y mantenible**

Esta arquitectura permite que:
- 👥 Varios desarrolladores trabajen en paralelo sin conflictos
- 🧪 Sea fácil escribir tests unitarios
- 🔧 Se agreguen features sin quebrar código existente
- 📚 El código sea autodocumentado

---

**[Volver a README →](../README.md)**
