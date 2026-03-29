# 📋 Documentación de Requisitos

**EspaceX_api - Aplicación de Escritorio WPF**

---

## 1. Descripción General

**EspaceX_api** es una aplicación de escritorio desarrollada en C# con WPF (.NET 8) que consume la **SpaceX API v4** para presentar datos en tiempo real sobre lanzamientos espaciales, cohetes y sitios de lanzamiento con un mapa interactivo.

La aplicación permite a los usuarios explorar información sobre:
- Historial completo de lanzamientos de SpaceX
- Especificaciones técnicas de cohetes
- Ubicaciones geográficas de sitios de lanzamiento
- Estadísticas y análisis por sitio

---

## 2. Requisitos Funcionales

### RF1: Vista de Inicio (Home)

| Aspecto | Descripción |
|--------|-----------|
| **Funcionalidad** | Dashboard interactivo que muestra acceso a tres secciones principales |
| **Elementos** | Tarjetas con iconos y descripción para Lanzamientos, Cohetes y Mapa |
| **Interacción** | Clic en tarjeta navega a la sección correspondiente |
| **Estética** | Fondo espacial con animaciones de estrellas y cohetes |

### RF2: Vista de Lanzamientos

| Aspecto | Descripción |
|--------|-----------|
| **Funcionalidad** | Mostrar tabla de lanzamientos de SpaceX con filtros |
| **Datos mostrados** | Nombre, Fecha, Estado (Exitoso/Fallido/Próximo), Detalles |
| **Carga de datos** | Botón "Cargar" consulta SpaceX API v4 `/launches` |
| **Filtro 1** | Búsqueda por texto en nombre y detalles (insensible a mayúsculas) |
| **Filtro 2** | Filtro por estado: Todos / Próximo / Exitoso / Fallido |
| **Indicadores** | Badges de color por estado en cada fila |
| **Actualización** | Botón "Actualizar" limpia caché de 1 hora y recarga |
| **Navegación** | Botón "Volver" retorna al Home |
| **Prioridad** | Alta |

### RF3: Vista de Cohetes

| Aspecto | Descripción |
|--------|-----------|
| **Funcionalidad** | Mostrar tabla de especificaciones técnicas de cohetes |
| **Datos mostrados** | Nombre, Altura, Masa, Costo, Tasa de éxito, Motores, Estado (Activo/Inactivo) |
| **Carga de datos** | Botón "Cargar" consulta SpaceX API v4 `/rockets` |
| **Actualización** | Botón "Actualizar" limpia caché y recarga |
| **Navegación** | Botón "Volver" retorna al Home |
| **Prioridad** | Alta |

### RF4: Vista de Mapa Interactivo

| Aspecto | Descripción |
|--------|-----------|
| **Funcionalidad** | Mapa mundial interactivo que muestra sitios de lanzamiento |
| **Carga** | Botón "Cargar Sitios" consulta SpaceX API v4 `/launchpads` |
| **Proyección** | Mercator (coordenadas geográficas a píxeles de pantalla) |
| **Continentes** | 7 continentes dibujados: N. América, S. América, Europa, África, Asia, Australia, Groenlandia |
| **Grilla** | Líneas de latitud y longitud cada 30° para referencia geográfica |
| **Zoom** | Rueda del mouse o botones Zoom+/Zoom- (rango 0.5x a 5.0x) |
| **Pan** | Arrastrar con botón izquierdo del mouse para navegar |
| **Reset** | Botón "Reset" vuelve a zoom y posición originales |
| **Puntos** | Punto rojo con halo solo cuando se selecciona sitio de la lista |
| **Sidebar** | Lista de sitios de lanzamiento + tabla de últimos 20 lanzamientos del sitio |
| **Indicador carga** | Overlay semitransparente con ProgressBar mientras API responde |
| **Navegación** | Botón "Volver" retorna al Home |
| **Prioridad** | Alta |

### RF5: Botón de Actualización Manual

| Aspecto | Descripción |
|--------|-----------|
| **Funcionalidad** | Botón "Actualizar" en LaunchesView y RocketsView |
| **Efecto** | Limpia caché y recarga datos desde API |
| **Ubicación** | Toolbar junto a botón "Cargar" |
| **Prioridad** | Media |

### RF6: Manejo de Errores

| Aspecto | Descripción |
|--------|-----------|
| **Error de API** | Si API falla, mostrar mensaje rojo con descripción del error |
| **Error de conexión** | Indicar al usuario si no hay conexión a internet |
| **Estado de carga** | Mostrar indicador visual mientras se cargan datos |
| **Prioridad** | Alta |

### RF7: Navegación Global

| Aspecto | Descripción |
|--------|-----------|
| **Patrón** | MVVM con ContentControl + DataTemplate |
| **Flujo** | Home ↔ (Lanzamientos / Cohetes / Mapa) |
| **Transiciones** | Instántaneas sin animaciones de cambio de vista |
| **Botón "Volver"** | Disponible en las tres vistas secundarias |
| **Prioridad** | Alta |

---

## 3. Requisitos No-Funcionales

### RNF1: Performance

| Aspecto | Especificación |
|--------|----------------|
| **Carga de lanzamientos** | < 3 segundos desde API |
| **Carga de cohetes** | < 2 segundos desde API |
| **Carga de sitios** | < 2 segundos desde API |
| **Tiempo de filtrado** | < 200ms (caché local) |
| **Zoom/Pan** | Smooth sin lag, respuesta instantánea |

### RNF2: Disponibilidad

| Aspecto | Especificación |
|--------|----------------|
| **Uptime esperado** | Mientras SpaceX API esté disponible |
| **Caché offline** | App funciona con datos cacheados sin internet (1 hora) |
| **Manejo de desconexión** | Error elegante sin crashes |

### RNF3: Usabilidad

| Aspecto | Especificación |
|--------|----------------|
| **Idioma** | Español (interfaz completamente localizada) |
| **Navegación intuitiva** | Máximo 2 clics para llegar a cualquier dato |
| **Iconos/Badges** | Código de colores para estados |
| **Accesibilidad** | Controles visibles, texto legible en tema oscuro |

### RNF4: Portabilidad

| Aspecto | Especificación |
|--------|----------------|
| **Sistema operativo** | Windows 10/11 |
| **Framework** | .NET 8 (cross-platform en teoría, WPF solo Windows) |
| **Instalación** | Ejecutable directo, sin dependencias externas |

### RNF5: Mantenibilidad

| Aspecto | Especificación |
|--------|----------------|
| **Patrón arquitectónico** | MVVM (separación clara View/ViewModel/Model) |
| **Principios SOLID** | Aplicados en toda la arquitectura |
| **Inyección de dependencias** | Microsoft.Extensions.DependencyInjection |
| **Código limpio** | Convenciones de nomenclatura, responsabilidad única |

### RNF6: Seguridad

| Aspecto | Especificación |
|--------|----------------|
| **Autenticación API** | SpaceX API v4 es pública, sin autenticación requerida |
| **Validación de entrada** | Filtros validan entrada del usuario |
| **Inyección SQL** | No aplica (API pública, sin BD) |
| **Certificados HTTPS** | Conexión segura a SpaceX API |

### RNF7: Caché de Datos

| Aspecto | Especificación |
|--------|----------------|
| **Mecanismo** | Caché en memoria con expiración por tiempo |
| **Datos cacheados** | Lanzamientos y Cohetes |
| **Tiempo de expiración** | 1 hora (3600 segundos) |
| **Propósito** | Reducir llamadas a API, mejorar UX, tolerar desconexiones cortas |
| **Control manual** | Botón "Actualizar" limpia caché forzando consulta API |
| **Implementación** | SpaceXApiService.cs con Dictionary y timestamps |

---

## 4. Casos de Uso Principales

### CU1: Explorar Lanzamientos

```
Actor: Usuario
Precondición: App abierta en Home
Flujo:
  1. Usuario hace clic en tarjeta "Lanzamientos"
  2. Sistema navega a LaunchesView
  3. Usuario presiona botón "Cargar"
  4. Sistema consulta API /launches
  5. Sistema muestra tabla con ~150 lanzamientos
  6. Usuario puede filtrar por texto o estado
  7. Usuario vuelve a Home presionando "Volver"
Postcondición: Tabla cargada y filtrada
```

### CU2: Explorar Cohetes

```
Actor: Usuario
Precondición: App abierta en Home
Flujo:
  1. Usuario hace clic en tarjeta "Cohetes"
  2. Sistema navega a RocketsView
  3. Usuario presiona botón "Cargar"
  4. Sistema consulta API /rockets
  5. Sistema muestra tabla con especificaciones de cohetes
  6. Usuario puede revisar datos técnicos (altura, masa, costo, tasa éxito)
  7. Usuario vuelve a Home presionando "Volver"
Postcondición: Tabla cargada y visible
```

### CU3: Explorar Mapa de Sitios

```
Actor: Usuario
Precondición: App abierta en Home
Flujo:
  1. Usuario hace clic en tarjeta "Mapa"
  2. Sistema navega a MapView (muestra continentes vacío)
  3. Usuario presiona botón "Cargar Sitios"
  4. Sistema consulta API /launchpads
  5. Sistema carga lista de sitios en sidebar
  6. Usuario selecciona un sitio en lista
  7. Sistema dibuja punto rojo en mapa para ese sitio
  8. Sistema muestra últimos 20 lanzamientos del sitio en tabla inferior
  9. Usuario hace zoom/pan para explorar
  10. Usuario vuelve a Home presionando "Volver"
Postcondición: Mapa interactivo funcional
```

### CU4: Usar Caché

```
Actor: Usuario
Precondición: Primera carga de lanzamientos hace < 1 hora
Flujo:
  1. Usuario vuelve a Home
  2. Usuario navega a Lanzamientos nuevamente
  3. Usuario presiona "Cargar"
  4. Sistema detecta que caché es válida (< 1 hora)
  5. Sistema devuelve datos del caché instantáneamente
Postcondición: Carga rápida sin consultar API
```

### CU5: Forzar Actualización

```
Actor: Usuario
Precondición: Datos cacheados hace 30 minutos
Flujo:
  1. Usuario presiona botón "Actualizar" en LaunchesView
  2. Sistema limpia caché de lanzamientos
  3. Sistema consulta API nuevamente
  4. Sistema muestra datos frescos
Postcondición: Caché actualizado, datos nuevos visible
```

---

## 5. Restricciones y Limitaciones

| Restricción | Descripción |
|------------|-----------|
| **API Rate Limiting** | SpaceX API sin límite de rate (pública sin autenticación) |
| **Datos en tiempo real** | Datos toman 1 hora de caché; cambios en SpaceX API no inmediatos |
| **Plataforma** | Solo Windows (WPF) |
| **Responsividad** | No es app móvil ni responsive a cambios de tamaño ventana |
| **Persistencia** | Datos no se guardan localmente, solo en caché en memoria |
| **Idioma fijo** | Solo español, sin soporte multiidioma |

---

## 6. Definición de Completitud

### La aplicación se considera "terminada" cuando:

✅ Compila sin errores (Error List vacía)  
✅ Todas las tres vistas cargan datos sin excepciones  
✅ Filtros funcionan correctamente  
✅ Navegación funciona sin dependencias circulares  
✅ Caché expira correctamente después de 1 hora  
✅ Mapa dibuja continentes y permite zoom/pan  
✅ Errores de API se muestran al usuario sin crashes  
✅ Code-behind mínimo, lógica en ViewModels  
✅ MVVM correctamente aplicado  

---

## Traceabilidad Requisito-Implementación

| Requisito | Archivo | Estado |
|-----------|---------|--------|
| RF1 - Home | HomeView.xaml / HomeViewModel.cs | ✅ Completo |
| RF2 - Lanzamientos | LaunchesView.xaml / LaunchesViewModel.cs | ✅ Completo |
| RF3 - Cohetes | RocketsView.xaml / RocketsViewModel.cs | ✅ Completo |
| RF4 - Mapa | MapView.xaml / MapViewModel.cs | ✅ Completo |
| RF5 - Caché | SpaceXApiService.cs | ✅ Completo |
| RF6 - Errores | Views (StringToVisibilityConverter) | ✅ Completo |
| RF7 - Navegación | MainViewModel.cs / MainWindow.xaml | ✅ Completo |

---