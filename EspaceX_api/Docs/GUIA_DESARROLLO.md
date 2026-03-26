# 🛠️ Guía de Desarrollo

## Tabla de Contenidos

1. [Configuración Inicial](#configuración-inicial)
2. [Agregar una Propiedad Observable](#agregar-una-propiedad-observable)
3. [Agregar un Nuevo Comando](#agregar-un-nuevo-comando)
4. [Flujo de Trabajo Git](#flujo-de-trabajo-git)
5. [Testing](#testing)

---

## Configuración Inicial

### Clonar y abrir

```bash
# Clonar el repositorio
git clone <tu-repo>
cd EspaceX_api

# Abrir en Visual Studio
# File → Open → Project/Solution → EspaceX_api.csproj
```

### Restaurar dependencias

Visual Studio automáticamente restaura NuGet, pero puedes hacerlo manualmente:

```bash
dotnet restore
```

### Compilar y ejecutar

```bash
dotnet build
dotnet run
# o presiona F5 en Visual Studio
```

---

## Agregar una Propiedad Observable

Con **MVVM Toolkit**, es muy simple.

### Paso 1: Agregar el atributo `[ObservableProperty]`

```csharp
using CommunityToolkit.Mvvm.ComponentModel;

public partial class MyViewModel : ObservableObject
{
    [ObservableProperty]
    private string myProperty = "valor inicial";
    
    // ¡Eso es todo! El toolkit genera automáticamente:
    // - El backing field (private)
    // - La propiedad pública (MyProperty)
    // - El PropertyChanged (para XAML)
}
```

### Paso 2: Usarla en XAML

```xaml
<TextBlock Text="{Binding MyViewModel.MyProperty}" />
```

### Paso 3: Modificarla en código

```csharp
MyProperty = "nuevo valor"; // Automáticamente notifica UI
```

### Con Observer (método que se ejecuta cuando cambia)

```csharp
[ObservableProperty]
private string searchText = string.Empty;

// Este método se ejecuta automáticamente cuando SearchText cambia
partial void OnSearchTextChanged(string value)
{
    ApplyFilters();
}
```

---

## Agregar un Nuevo Comando

Los comandos son acciones que el usuario dispara (botones, etc).

### Paso 1: Usar atributo `[RelayCommand]`

```csharp
using CommunityToolkit.Mvvm.Input;

public partial class MyViewModel : ObservableObject
{
    [RelayCommand]
    public async Task MyCommand()
    {
        // Tu código aquí
        await Task.Delay(1000);
    }
}

// ¡Eso es todo! El toolkit genera automáticamente:
// - Propiedad MyCommandCommand (tipo RelayCommand)
// - PropertyChanged notifications
```

### Paso 2: Bindearlo en XAML

```xaml
<Button Command="{Binding MyCommandCommand}" 
        Content="Click aquí" />
```

### Con parámetro

```csharp
[RelayCommand]
public async Task DeleteLaunch(string launchId)
{
    await _apiService.DeleteAsync(launchId);
    // ...
}
```

```xaml
<Button Command="{Binding DeleteLaunchCommand}"
        CommandParameter="{Binding SelectedLaunch.Id}" />
```

---

## Flujo de Trabajo Git

### Gitflow Recomendado

```bash
# 1. Crear rama feature desde develop
git checkout develop
git pull origin develop
git checkout -b feature/launches

# 2. Trabajar en tu feature
# ... editar LaunchesView.xaml y LaunchesViewModel.cs

# 3. Commit y push
git add .
git commit -m "feat: agregar tabla de lanzamientos con filtros"
git push origin feature/launches

# 4. Pull Request a develop
# En GitHub: crear PR de feature/launches → develop
# El líder revisa y mergea a develop

# 5. Después: merge develop → main (release)
```

### Estructura de Commits Recomendada

```bash
git commit -m "feat: agregar SearchText en LaunchesViewModel"
git commit -m "feat: agregar filtro por estado"
git commit -m "fix: corregir visualización de fechas"
git commit -m "docs: actualizar README"
```

---

## Testing

### Crear clase Mock para testing

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using EspaceX_api.Models;
using EspaceX_api.Services;

namespace EspaceX_api.Tests
{
    /// <summary>
    /// Mock para testing sin hacer llamadas reales a la API.
    /// </summary>
    public class MockSpaceXApiService : ISpaceXApiService
    {
        public Task<List<LaunchModel>> GetLaunchesAsync()
        {
            return Task.FromResult(new List<LaunchModel>
            {
                new LaunchModel
                {
                    Id = "1",
                    Name = "Falcon 9 Test",
                    DateUtc = System.DateTime.UtcNow,
                    Success = true,
                    Upcoming = false
                }
            });
        }

        public Task<List<RocketModel>> GetRocketsAsync()
        {
            return Task.FromResult(new List<RocketModel>
            {
                new RocketModel { Id = "1", Name = "Falcon 9", Active = true }
            });
        }

        public Task<LaunchpadModel> GetLaunchpadAsync(string launchpadId) 
            => Task.FromResult<LaunchpadModel>(null);
        
        public Task<RocketModel> GetRocketAsync(string rocketId) 
            => Task.FromResult<RocketModel>(null);
        
        public void ClearCache() { }
        public bool IsCacheExpired() => false;
    }
}
```

### Usar el Mock en testing

```csharp
using Xunit;

namespace EspaceX_api.Tests
{
    public class LaunchesViewModelTests
    {
        [Fact]
        public async Task LoadLaunches_ShouldPopulateLaunches()
        {
            // Arrange
            var mockService = new MockSpaceXApiService();
            var viewModel = new LaunchesViewModel(mockService);

            // Act
            await viewModel.LoadLaunches();

            // Assert
            Assert.NotEmpty(viewModel.Launches);
            Assert.Equal(1, viewModel.Launches.Count);
        }
    }
}
```

---

## Tips para Trabajo en Equipo

✅ **Antes de empezar**:
```bash
git pull origin develop
```

✅ **Al terminar tu feature**:
```bash
git push origin feature/xxxxx
# Hacer Pull Request a develop en GitHub
# Esperar revisión del líder
```

✅ **Si hay conflictos**:
- Comunicarse con el líder
- Resolver conflictos juntos
- Validar que no se rompa nada

✅ **Commits significativos**:
- No hacer commits mega-grandes
- Dividir en cambios lógicos
- Escribir mensajes claros

---

## Debugging

### Puntos de ruptura (Breakpoints)

```csharp
[RelayCommand]
public async Task LoadLaunches()
{
    IsLoading = true;
    // Presiona F9 aquí para poner un breakpoint
    // Luego presiona F5 para ejecutar y se pausará aquí
    try
    {
        var launches = await _apiService.GetLaunchesAsync();
        Launches = new ObservableCollection<LaunchModel>(launches);
    }
    finally
    {
        IsLoading = false;
    }
}
```

### Inspeccionar variables en runtime

1. Pausa el debugger (F5 en un breakpoint)
2. Hover sobre la variable para ver su valor
3. O abre **Debug → Windows → Locals** para ver todas las variables

---

**[Volver a README →](../README.md)**
