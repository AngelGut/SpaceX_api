# EspaceX API — Aplicación de Escritorio

**ID de Documento:** DOC-ESPACEX-README-001  
**Versión:** 1.0.0  
**Estado:** Publicado  
**Fecha:** 2026-03-29  

---

## Tabla de Contenidos

1. [Descripción General](#1-descripción-general)  
2. [Alcance](#2-alcance)  
3. [Definiciones, Acrónimos y Abreviaturas](#3-definiciones-acrónimos-y-abreviaturas)  
4. [Requisitos del Sistema](#4-requisitos-del-sistema)  
   - 4.1 [Dependencias de Software](#41-dependencias-de-software)  
   - 4.2 [Requisitos de Hardware](#42-requisitos-de-hardware)  
   - 4.3 [Requisitos de Red](#43-requisitos-de-red)  
5. [Instalación y Configuración](#5-instalación-y-configuración)  
6. [Estructura del Proyecto](#6-estructura-del-proyecto)  
7. [Descripción de la Arquitectura](#7-descripción-de-la-arquitectura)  
8. [Descripción Funcional](#8-descripción-funcional)  
9. [Atributos de Calidad](#9-atributos-de-calidad)  
10. [Lineamientos de Desarrollo](#10-lineamientos-de-desarrollo)  
    - 10.1 [Estrategia de Ramas](#101-estrategia-de-ramas)  
    - 10.2 [Convenciones de Commits](#102-convenciones-de-commits)  
11. [Índice de Documentación](#11-índice-de-documentación)  
12. [Control del Documento](#12-control-del-documento)  

---

## 1. Descripción General

**EspaceX API** es una aplicación de escritorio para Windows, desarrollada con WPF y .NET 8, que consume la API pública de SpaceX v4 y presenta sus datos de forma visual e interactiva. Permite explorar el historial de lanzamientos, especificaciones técnicas de cohetes y la ubicación geográfica de los sitios de lanzamiento.

La aplicación implementa el patrón arquitectónico **Modelo–Vista–VistaModelo (MVVM)**, respeta los principios SOLID y hace uso del contenedor de Inyección de Dependencias de Microsoft junto con la librería MVVM Toolkit.

---

## 2. Alcance

Este documento cubre:

- Requisitos de sistema y software necesarios para compilar y ejecutar la aplicación
- Procedimientos de instalación y configuración del entorno de desarrollo
- Estructura del código fuente y decisiones arquitectónicas
- Descripción funcional de los módulos de la aplicación
- Estándares de flujo de trabajo y desarrollo en equipo

Este documento **no** cubre:

- Manual operativo para el usuario final (véase `MANUAL_USUARIO.docx`)
- Referencia detallada de integración con la API (véase `API_REFERENCE.md`)
- Procedimientos de despliegue en entornos distintos al local

---

## 3. Definiciones, Acrónimos y Abreviaturas

| Término | Definición |
|---------|-----------|
| **WPF** | Windows Presentation Foundation — marco de trabajo para interfaces de escritorio en Windows (.NET) |
| **MVVM** | Modelo–Vista–VistaModelo — patrón arquitectónico que separa la lógica de presentación de la lógica de negocio |
| **DI** | Inyección de Dependencias — patrón de diseño para desacoplar la instanciación de componentes |
| **DTO** | Objeto de Transferencia de Datos — objeto utilizado para mapear respuestas JSON a modelos internos |
| **HTTP** | Protocolo de Transferencia de Hipertexto — protocolo de capa de aplicación para comunicación con la API |
| **API** | Interfaz de Programación de Aplicaciones |
| **SOLID** | Conjunto de cinco principios de diseño orientado a objetos (Responsabilidad Única, Abierto/Cerrado, Sustitución de Liskov, Segregación de Interfaces, Inversión de Dependencias) |
| **NuGet** | Gestor de paquetes para .NET |
| **SDK** | Kit de Desarrollo de Software |
| **TTL** | Tiempo de Vida — duración máxima de validez de datos en caché |

---

## 4. Requisitos del Sistema

### 4.1 Dependencias de Software

| Componente | Versión Mínima | Notas |
|-----------|---------------|-------|
| Sistema Operativo | Windows 10 (x64) | WPF es exclusivo de Windows |
| .NET SDK | 8.0 | Requerido para compilación y ejecución |
| Visual Studio | 2022 (cualquier edición) | IDE recomendado |
| Git | 2.x | Control de versiones |

**Paquetes NuGet (se resuelven automáticamente):**

| Paquete | Propósito |
|---------|----------|
| `CommunityToolkit.Mvvm` | Generadores de código MVVM y clases base |
| `Microsoft.Extensions.DependencyInjection` | Contenedor de Inyección de Dependencias |
| `Microsoft.Extensions.Http` | Soporte para `IHttpClientFactory` |

### 4.2 Requisitos de Hardware

| Recurso | Mínimo |
|---------|--------|
| Memoria RAM | 4 GB |
| Espacio en disco | 500 MB (incluye el entorno de ejecución .NET) |
| Resolución de pantalla | 1280 × 720 px |

### 4.3 Requisitos de Red

| Requisito | Detalle |
|-----------|---------|
| Conexión a internet | Obligatoria en tiempo de ejecución para consumir la API de SpaceX |
| Endpoint de destino | `https://api.spacexdata.com/v4` |
| Autenticación | No requerida — API pública sin credenciales |

---

## 5. Instalación y Configuración

### Paso 1 — Clonar el Repositorio

```bash
git clone <url-del-repositorio>
cd EspaceX_api
```

### Paso 2 — Abrir la Solución

Abrir Visual Studio 2022 y navegar a:

```
Archivo → Abrir → Proyecto/Solución → EspaceX_api.csproj
```

### Paso 3 — Restaurar Paquetes NuGet

Visual Studio realiza este paso automáticamente al cargar el proyecto. Para restaurar de forma manual:

```bash
dotnet restore
```

### Paso 4 — Compilar

```bash
dotnet build
```

Resultado esperado: `Build succeeded. 0 Warning(s). 0 Error(s).`

### Paso 5 — Ejecutar

```bash
dotnet run
```

Alternativamente, presionar **F5** en Visual Studio para iniciar con el depurador.

> **Nota:** Se requiere conexión a internet activa. La aplicación realiza llamadas HTTP a la API de SpaceX al iniciar y al navegar entre vistas.

---

## 6. Estructura del Proyecto

```
EspaceX_api/
│
├── Models/                        # Entidades de dominio (objetos C# puros, sin atributos JSON)
│   ├── LaunchModel.cs
│   ├── RocketModel.cs
│   ├── LaunchpadModel.cs
│   └── MapPointModel.cs
│
├── Services/                      # Capa de acceso a datos
│   ├── ISpaceXApiService.cs       # Contrato del servicio (interfaz)
│   ├── SpaceXApiService.cs        # Implementación HTTP
│   └── DTOs/                      # Objetos de mapeo JSON → Modelo
│       ├── LaunchDto.cs
│       ├── RocketDto.cs
│       └── LaunchpadDto.cs
│
├── ViewModels/                    # Lógica de presentación (MVVM)
│   ├── MainViewModel.cs           # Coordinador de navegación
│   ├── HomeViewModel.cs
│   ├── LaunchesViewModel.cs
│   ├── RocketsViewModel.cs
│   └── MapViewModel.cs
│
├── Views/                         # Interfaz de usuario (XAML)
│   ├── MainWindow.xaml
│   ├── HomeView.xaml
│   ├── LaunchesView.xaml
│   ├── RocketsView.xaml
│   └── MapView.xaml
│
├── Docs/                          # Documentación del proyecto
│   ├── README.md                  # Este documento
│   ├── ARQUITECTURA.md
│   ├── GUIA_DESARROLLO.md
│   ├── API_REFERENCE.md
│   ├── INSTALACION_SETUP.md
│   └── MANUAL_USUARIO.docx
│
├── App.xaml                       # Diccionarios de recursos y estilos globales
├── App.xaml.cs                    # Configuración del contenedor DI y arranque
└── EspaceX_api.csproj             # Definición del proyecto y referencias NuGet
```

---

## 7. Descripción de la Arquitectura

La aplicación implementa un patrón MVVM estricto de tres capas:

| Capa | Componentes | Responsabilidades |
|------|------------|------------------|
| **Vista** | Archivos `*.xaml` | Renderizar la interfaz; enlazar propiedades y comandos del VistaModelo |
| **VistaModelo** | Archivos `*ViewModel.cs` | Gestionar el estado de la UI; procesar comandos del usuario; coordinar llamadas al servicio |
| **Servicio** | `SpaceXApiService.cs` | Ejecutar peticiones HTTP; deserializar DTOs; gestionar caché |
| **Modelo** | Archivos `*Model.cs` | Representar entidades de dominio; sin dependencias de marco de trabajo |

**Decisiones de diseño destacadas:**

- La interfaz `ISpaceXApiService` permite sustituir la implementación y facilita las pruebas unitarias sin modificar los consumidores (Principio Abierto/Cerrado).
- El contenedor DI se configura en `App.xaml.cs`, registrando `HttpClient` y servicios como singletons.
- El caché en memoria con TTL de 1 hora reduce llamadas innecesarias a la API. El método `ClearCache()` fuerza la actualización de datos.
- Los atributos `[ObservableProperty]` y `[RelayCommand]` del MVVM Toolkit eliminan el código repetitivo de notificación de cambios.

Para la especificación arquitectónica completa, véase [`ARQUITECTURA.md`](./ARQUITECTURA.md).

---

## 8. Descripción Funcional

| Módulo | Descripción |
|--------|------------|
| **Panel Principal (Home)** | Vista resumen con métricas clave agregadas de todas las fuentes de datos |
| **Lanzamientos** | Historial completo de lanzamientos con búsqueda por texto y filtros por estado (exitoso / fallido / próximo) |
| **Cohetes** | Especificaciones técnicas por cohete: altura, masa, costo por lanzamiento y tasa de éxito |
| **Mapa Interactivo** | Visualización geográfica de sitios de lanzamiento con coordenadas obtenidas del endpoint de plataformas |

---

## 9. Atributos de Calidad

Los siguientes atributos de calidad han sido abordados en conformidad con la norma ISO/IEC 25010:2011:

| Característica | Implementación |
|----------------|---------------|
| **Mantenibilidad** | Separación estricta por capas MVVM; cada capa es modificable de forma independiente |
| **Capacidad de prueba** | Servicios abstraídos detrás de interfaces; ViewModels inyectables |
| **Eficiencia de rendimiento** | Caché en memoria con TTL de 1 hora; operaciones asíncronas con `async/await` |
| **Adecuación funcional** | Cobertura de todos los endpoints públicos de SpaceX v4 relevantes al alcance definido |
| **Fiabilidad** | Errores HTTP propagados a la interfaz de usuario; sin fallos silenciosos |

---

## 10. Lineamientos de Desarrollo

### 10.1 Estrategia de Ramas

El proyecto sigue el modelo **Gitflow**:

```bash
# Crear rama de funcionalidad desde develop
git checkout develop
git pull origin develop
git checkout -b feature/<descripcion-corta>

# Realizar cambios y confirmarlos
git add .
git commit -m "feat: <descripción>"
git push origin feature/<descripcion-corta>

# Abrir un Pull Request hacia develop
```

**Convención de nombres de ramas:**

| Prefijo | Uso |
|---------|-----|
| `feature/` | Nueva funcionalidad |
| `fix/` | Corrección de defectos |
| `docs/` | Cambios exclusivamente en documentación |
| `refactor/` | Reestructuración de código sin cambio de comportamiento |
| `release/` | Preparación de una versión para producción |

### 10.2 Convenciones de Commits

Los mensajes de commit siguen la especificación **Conventional Commits 1.0.0**:

```
<tipo>(<ámbito opcional>): <descripción corta>
```

| Tipo | Uso |
|------|-----|
| `feat` | Introduce una nueva funcionalidad |
| `fix` | Corrige un defecto |
| `docs` | Cambios únicamente en documentación |
| `refactor` | Cambio de código sin impacto funcional |
| `test` | Adición o modificación de pruebas |
| `chore` | Proceso de build, actualización de dependencias, herramientas |

**Ejemplos:**

```
feat(lanzamientos): agregar filtro desplegable por estado en LaunchesView
fix(mapa): corregir análisis de coordenadas para sitios de lanzamiento polares
docs: actualizar API_REFERENCE con nuevos parámetros de paginación
```

---

## 11. Índice de Documentación

| Documento | Audiencia | Descripción |
|-----------|----------|-------------|
| `README.md` *(este archivo)* | Todos | Descripción general, instalación y estructura del proyecto |
| `INSTALACION_SETUP.md` | Desarrolladores (nuevos) | Guía detallada de configuración del entorno |
| `ARQUITECTURA.md` | Desarrolladores, Tech Leads | Arquitectura completa y decisiones de diseño |
| `GUIA_DESARROLLO.md` | Desarrolladores | Cómo extender la aplicación; guía de pruebas |
| `API_REFERENCE.md` | Desarrolladores | Referencia de métodos del servicio |
| `MANUAL_USUARIO.docx` | Usuarios finales, QA, Product Owners | Manual operativo de la aplicación |

**Ruta de lectura recomendada por rol:**

- **Desarrollador nuevo:** README → INSTALACION_SETUP → ARQUITECTURA → GUIA_DESARROLLO → API_REFERENCE
- **Tech Lead / Desarrollador Senior:** ARQUITECTURA → API_REFERENCE → código fuente
- **Product Owner / Analista:** README → MANUAL_USUARIO
- **QA / Tester:** README → MANUAL_USUARIO → GUIA_DESARROLLO (sección de pruebas)

---

## 12. Control del Documento

| Campo | Valor |
|-------|-------|
| **ID de Documento** | DOC-ESPACEX-README-001 |
| **Versión** | 1.0.0 |
| **Fecha** | 2026-03-29 |
| **Estado** | Publicado |
| **Autores** | Equipo de Desarrollo EspaceX |
| **Revisado por** | Angel David Gutierrez |

### Historial de Revisiones

| Versión | Fecha | Autor | Descripción |
|---------|-------|-------|-------------|
| 1.0.0 | 2026-03-29 | Angel David Gutierrez | Versión inicial |

---

*Este documento se mantiene bajo control de versiones. Para la revisión más reciente, consultar la rama `main` del repositorio del proyecto.*
