# 🚀 EspaceX_api - Aplicación WPF

**Aplicación moderna de escritorio para explorar datos en tiempo real de SpaceX.**

Desarrollada con **.NET 8**, **MVVM Toolkit**, **Inyección de Dependencias** y **Principios SOLID**.

---

## 📋 Tabla de Contenidos

| Sección | Descripción | Audiencia |
|---------|-------------|-----------|
| [¿Qué es?](#qué-es) | Visión general del proyecto | Todos |
| [Requisitos](#requisitos) | Dependencias y setup inicial | Desarrolladores |
| [Inicio Rápido](#inicio-rápido) | Clonar y ejecutar | Desarrolladores |
| [Estructura](#estructura) | Organización del código | Desarrolladores |
| [Documentación](#documentación) | Guías y referencias técnicas | Por rol |

---

## ¿Qué es?

**EspaceX_api** es una aplicación WPF que consume la **SpaceX API v4** y presenta datos de forma visual:

- **🛸 Lanzamientos**: Historial completo con búsqueda y filtros por estado
- **🚀 Cohetes**: Especificaciones técnicas (altura, masa, costo, tasa de éxito)
- **🌍 Mapa Interactivo**: Sitios de lanzamiento con coordenadas geográficas
- **🏠 Dashboard**: Resumen de información clave

### Stack Técnico

| Componente | Versión |
|-----------|---------|
| .NET | 8.0 |
| Framework | WPF (Windows Presentation Foundation) |
| UI Patterns | MVVM + MVVM Toolkit |
| Inyección DI | Microsoft.Extensions.DependencyInjection |
| HTTP | HttpClient |

---

## Requisitos

### Software

- **Visual Studio 2022** (Community, Professional o Enterprise)
- **.NET 8.0 SDK** o superior
- **Git** para control de versiones

### Acceso a API

- **SpaceX API v4** (sin autenticación requerida)
- Conexión a internet para consumir datos

---

## Inicio Rápido

### 1. Clonar el Repositorio

```bash
git clone <url-del-repositorio>
cd EspaceX_api
```

### 2. Abrir en Visual Studio

```
File → Open → Project/Solution
Seleccionar: EspaceX_api.csproj
```

### 3. Restaurar Dependencias

Visual Studio automáticamente restaura NuGet. Si no:

```bash
dotnet restore
```

### 4. Compilar y Ejecutar

```bash
dotnet build
dotnet run
```
---

## Estructura

```
EspaceX_api/
│
├── Models/                    # Datos puros (sin JSON)
│   ├── LaunchModel.cs
│   ├── RocketModel.cs
│   ├── LaunchpadModel.cs
│   └── MapPointModel.cs
│
├── Services/                  # Lógica de acceso a datos
│   ├── ISpaceXApiService.cs   # Interfaz (contrato)
│   ├── SpaceXApiService.cs    # Implementación
│   └── DTOs/                  # Mapeo JSON ↔ Models
│       ├── LaunchDto.cs
│       ├── RocketDto.cs
│       └── LaunchpadDto.cs
│
├── ViewModels/                # Lógica de presentación
│   ├── MainViewModel.cs       # Coordinador de navegación
│   ├── HomeViewModel.cs       # Dashboard
│   ├── LaunchesViewModel.cs   # Lanzamientos
│   ├── RocketsViewModel.cs    # Cohetes
│   └── MapViewModel.cs        # Mapa interactivo
│
├── Views/                     # Interfaz de usuario (XAML)
│   ├── MainWindow.xaml
│   ├── HomeView.xaml
│   ├── LaunchesView.xaml
│   ├── RocketsView.xaml
│   └── MapView.xaml
│
├── Docs/                      # Documentación
│   ├── README.md              # Este archivo
│   ├── ARQUITECTURA.md        # Diseño técnico
│   ├── GUIA_DESARROLLO.md     # Cómo extender
│   ├── API_REFERENCE.md       # Referencia de métodos
│   └── INSTALACION_SETUP.md   # Setup detallado
│
├── App.xaml                   # Estilos globales
├── App.xaml.cs                # Configuración DI
└── EspaceX_api.csproj         # Definición del proyecto
```

---

## Documentación

Según tu rol, comienza por:

### 👨‍💼 **Product Owners / Analistas**
```
1. Este README
2. MANUAL_USUARIO.docx
3. Opcionalmente: ARQUITECTURA.md (sección visión general)
```

### 👨‍💻 **Desarrolladores Nuevos**
```
1. Este README
2. INSTALACION_SETUP.md
3. ARQUITECTURA.md (entender diseño)
4. GUIA_DESARROLLO.md (cómo extender)
5. API_REFERENCE.md (cuando necesites)
```

### 🔧 **Desarrolladores Senior / Tech Leads**
```
1. ARQUITECTURA.md (completo)
2. API_REFERENCE.md (métodos específicos)
3. Revisar código fuente
```

### 🧪 **QA / Testers**
```
1. Este README
2. MANUAL_USUARIO.docx
3. GUIA_DESARROLLO.md (sección Testing)
```

---

## Flujo de Trabajo en Equipo

### Rama de Feature (Gitflow)

```bash
# 1. Crear rama desde develop
git checkout develop
git pull origin develop
git checkout -b feature/mi-feature

# 2. Trabajar y hacer commits
git add .
git commit -m "feat: descripción clara"

# 3. Push y Pull Request
git push origin feature/mi-feature
# → Crear PR a develop en GitHub
```

### Convenciones de Commits

```
feat: nueva funcionalidad
fix: corregir bug
docs: cambios en documentación
refactor: refactorizar código
test: agregar/modificar tests
chore: tareas de mantenimiento
```

---

## Características Principales

### ✅ Arquitectura MVVM

| Capa | Responsabilidad |
|------|-----------------|
| **Views** | Interfaz visual (XAML) |
| **ViewModels** | Lógica de presentación |
| **Services** | Acceso a datos |
| **Models** | Entidades de negocio |

### ✅ MVVM Toolkit

- `[ObservableProperty]`: Propiedades observables automáticas
- `[RelayCommand]`: Comandos sin boilerplate
- `partial void OnPropertyChanged()`: Métodos observadores

### ✅ Caché Inteligente

- Datos en memoria con expiración de 1 hora
- Reduce llamadas innecesarias a API
- `ClearCache()` fuerza actualización

---

## Soporte

- **Documentación**: Ver carpeta `/Docs`
- **Código ejemplo**: Ver `ViewModels/` y `Views/`

---

**Última actualización:29 Marzo 2026  
**Versión:** 1.0  
**Estado:** Producción