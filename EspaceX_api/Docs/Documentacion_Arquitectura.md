# 🏗️ Arquitectura de EspaceX_api

**Documentación de Arquitectura y Principios SOLID**

---

## 📋 Tabla de Contenidos

| Sección | Propósito | Audiencia |
|---------|-----------|-----------|
| [Visión General](#visión-general) | Cómo está organizado el código | Todos |
| [Inyección de Dependencias](#inyección-de-dependencias) | Cómo funciona DI | Desarrolladores |
| [Patrón MVVM](#patrón-mvvm) | Separación Vista-Lógica | Desarrolladores |
| [Flujo de Datos](#flujo-de-datos) | Cómo se mueven los datos | Desarrolladores |
| [Estructura de Carpetas](#estructura-de-carpetas) | Organización física | Todos |

---

## Visión General

EspaceX_api sigue una **arquitectura MVVM**:

```
┌─────────────────────────────────────────────┐
│         PRESENTATION LAYER                  │
│  Views (XAML) + ViewModels (Lógica)         │
└──────────────────┬──────────────────────────┘
                   │ Data Binding
┌──────────────────▼──────────────────────────┐
│      BUSINESS LOGIC LAYER                   │
│  ViewModels + Comandos + Filtros            │
└──────────────────┬──────────────────────────┘
                   │ Inyección
┌──────────────────▼──────────────────────────┐
│        SERVICE LAYER                        │
│  ISpaceXApiService (Interfaz)               │
│  SpaceXApiService (Implementación)          │
│  + Caché en memoria                         │
└──────────────────┬──────────────────────────┘
                   │ Mapeo
┌──────────────────▼──────────────────────────┐
│        DATA LAYER                           │
│  DTOs (Deserialización JSON)                │
│  Models (Datos puros)                       │
└──────────────────┬──────────────────────────┘
                   │ HTTP
┌──────────────────▼──────────────────────────┐
│        EXTERNAL API LAYER                   │
│  SpaceX API v4 (Sin autenticación)          │
└─────────────────────────────────────────────┘
```

### Ventajas de esta Arquitectura

| Ventaja | Beneficio |
|---------|-----------|
| **Separación de responsabilidades** | Cada parte tiene un propósito claro |
| **Testabilidad** | Fácil mock de servicios |
| **Mantenibilidad** | Cambios localizados |
| **Escalabilidad** | Agregar features sin romper existentes |
| **Trabajo en equipo** | Menos conflictos en Git |

---

## Inyección de Dependencias

### ¿Qué es DI?

| Aspecto | Descripción |
|--------|-----------|
| **Sin DI** | `var service = new SpaceXApiService();` (tight coupling) |
| **Con DI** | `ISpaceXApiService service` recibida en constructor |
| **Ventaja** | Desacoplamiento, testabilidad, flexibilidad |

### Ciclos de Vida

| Ciclo | Comportamiento |
|------|---|
| **Singleton** | Una sola instancia para toda la app (ej: servicios) |
| **Transient** | Nueva instancia cada vez que se pida (ej: viewmodels) |
| **Scoped** | Una instancia por "scope" (ej: request en web) |

**En este proyecto usamos Singleton** para mantener caché de datos.

---

## Patrón MVVM

### Estructura MVVM

```
View (XAML)                         ← Usuario ve
    ↕ Data Binding                    ↓ Interactúa
ViewModel (Lógica)                  ← Estado + Comandos
    ↕ Método llamada                  ↓ Llama
Service (Datos)                     ← API + Caché
    ↕ HTTP                            ↓
API Externa                         ← JSON
```

### Responsabilidades

| Layer | Responsabilidad | Ejemplo |
|-------|---------|---------|
| **View** | Mostrar datos, capturar entrada | XAML, Botones, TextBox |
| **ViewModel** | Lógica, estado, comandos | Filtros, búsqueda, carga |
| **Service** | Acceso a datos, HTTP | Llamadas a API, caché |

### MVVM Toolkit

| Atributo | Genera |
|----------|--------|
| `[ObservableProperty]` | Propiedad observable + backing field + PropertyChanged |
| `[RelayCommand]` | Comando observable + property + CanExecute |
| `partial void OnPropertyChanged()` | Método que se ejecuta cuando cambia una propiedad |

---

## Flujo de Datos

### 1. Usuario Carga Lanzamientos

```
Usuario hace clic "Cargar"
    ↓
XAML Button → Command binding
    ↓
LaunchesViewModel.LoadLaunchesCommand
    ↓ async/await
ISpaceXApiService.GetLaunchesAsync()
    ↓ HTTP request
SpaceX API v4
    ↓ JSON response
JsonSerializer.Deserialize<List<LaunchDto>>()
    ↓ mapeo (DTO → Model)
DTO → LaunchModel (quita datos no necesarios)
    ↓
LaunchesViewModel.Launches = ObservableCollection(result)
    ↓ PropertyChanged notification
XAML DataGrid observa cambio
    ↓
UI actualiza automáticamente
    ↓
Usuario ve datos
```

### 2. Usuario Busca por Nombre

```
Usuario escribe en TextBox (SearchText)
    ↓
PropertyChanged → OnSearchTextChanged()
    ↓
ApplyFilters() ejecuta LINQ
    ↓
FilteredLaunches = resultado filtrado
    ↓
PropertyChanged notification
    ↓
XAML DataGrid observa cambio
    ↓
UI actualiza (solo mostrando coincidencias)
```

---

## Estructura de Carpetas

### Raíz del Proyecto

```
EspaceX_api/
├── Models/                          # Entidades de negocio
│   ├── LaunchModel.cs               # Lanzamiento de SpaceX
│   ├── RocketModel.cs               # Especificaciones de cohete
│   ├── LaunchpadModel.cs            # Sitio de lanzamiento
│   └── MapPointModel.cs             # Punto en mapa
│
├── Services/                        # Acceso a datos
│   ├── ISpaceXApiService.cs         # Contrato (interfaz)
│   ├── SpaceXApiService.cs          # Implementación (API + caché)
│   └── DTOs/                        # Data Transfer Objects
│       ├── LaunchDto.cs             # Mapeo exacto con JSON
│       ├── RocketDto.cs             # Mapeo exacto con JSON
│       └── LaunchpadDto.cs          # Mapeo exacto con JSON
│
├── ViewModels/                      # Lógica de presentación
│   ├── MainViewModel.cs             # Coordinador de navegación
│   ├── HomeViewModel.cs             # Dashboard/inicio
│   ├── LaunchesViewModel.cs         # Pestaña lanzamientos
│   ├── RocketsViewModel.cs          # Pestaña cohetes
│   └── MapViewModel.cs              # Pestaña mapa
│
├── Views/                           # Interfaz (XAML + code-behind)
│   ├── MainWindow.xaml              # Ventana principal (shell)
│   ├── MainWindow.xaml.cs           # Code-behind mínimo
│   ├── HomeView.xaml                # Vista inicio
│   ├── HomeView.xaml.cs             # Code-behind (mínimo)
│   ├── LaunchesView.xaml            # Vista lanzamientos
│   ├── LaunchesView.xaml.cs         # Code-behind (mínimo)
│   ├── RocketsView.xaml             # Vista cohetes
│   ├── RocketsView.xaml.cs          # Code-behind (mínimo)
│   ├── MapView.xaml                 # Vista mapa
│   └── MapView.xaml.cs              # Code-behind + lógica
│
├── App.xaml                         # Estilos globales
├── App.xaml.cs                      # Configuración DI + conversores
├── EspaceX_api.csproj               # Configuración proyecto
│
└── Docs/                            # Documentación
    ├── README.md                    # Inicio rápido
    ├── ARQUITECTURA.md              # Este archivo
    ├── GUIA_DESARROLLO.md           # Cómo extender
    ├── API_REFERENCE.md             # Referencia técnica
    └── INSTALACION_SETUP.md         # Setup detallado
```

---

## Caché de Datos

### Ventajas

| Ventaja | Impacto |
|---------|--------|
| No golpea API innecesariamente | Reduce ancho de banda |
| UX rápida (datos locales) | Mejor respuesta de UI |
| Tolerancia a desconexión | Datos disponibles sin internet |

---

## Conclusión

EspaceX_api implementa:

✅ **Arquitectura en capas** bien definida  
✅ **Principios SOLID** en cada componente  
✅ **MVVM + MVVM Toolkit** para UI reactiva  
✅ **Inyección de Dependencias** para desacoplamiento  
✅ **Caché inteligente** de datos  
✅ **Interfaces claras** para extensión  

**Resultado:**
- Código testeable y mantenible
- Trabajo en equipo sin conflictos
- Fácil agregar features
- Profesional y escalable

---

**[← Volver a README](README.md)**