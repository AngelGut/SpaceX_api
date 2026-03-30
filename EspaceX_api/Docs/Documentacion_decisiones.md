# Documentación de Decisiones, Errores y Soluciones

**EspaceX_api - Registro de problemas encontrados, decisiones técnicas y lecciones aprendidas**

## Desiciones Imporantes de como planteamos el proyecto

# 1. Desicion de arquitecturaa seguir, por que elegimos MVVM, como separamos responsabilidades, etc

Decimo que el patron MVVM es el mas adecuado por su practicidad y facilidad de mantenimiento
Este es simple por el ToolKit MVVM, y hace que el codigo sea mucho mas facil de crear ya que tiene propiedades que pre crean todo
esto hace todo mucho mas simple...

# 2. Decicion de usar la api de SpaceX
Elegimos la api de SpaceX por su simplicidad, su buena documentacion y porque es una api publica que no requiere autenticacion
y como necesitabamos un proyecto simple fue una desicion divertida y acertada.

# 3. Decicion de usar WPF
WPF da una capacidad de creacion visual mucho mas grande que otras tecnologias, y es una tecnologia que se adapta muy bien a proyectos de escritorio
y nos deja darle un toque moderno de manera sencilla y no se vea como una app de 2003

---

## 1. Errores de Compilación

### Error #1: CS0101 - Tipo Duplicado en Namespace

**Síntoma:**
```
The namespace 'EspaceX_api' already contains a definition for 'StringToVisibilityConverter'
```

**Causa raíz:**
- La clase `StringToVisibilityConverter` fue definida en **dos archivos** dentro del mismo namespace
- Existía en `App.xaml.cs` (correctamente registrada como recurso global)
- Existía en `StringToVisibilityConverter.cs` (copia duplicada accidental)

**Solución:**
- Eliminar completamente el archivo `StringToVisibilityConverter.cs` del proyecto
- Mantener solo la definición en `App.xaml.cs`
- La clase está registrada en `Application.Resources` como `StaticResource` accesible desde todas las Views

**Impacto:**
- Bloqueante total - proyecto no compilaba
- Resuelto simultáneamente CS0101 y CS0111

**Lección:**
- Usar Solution Explorer > Delete para eliminar archivos (no solo del disco)
- Revisar siempre si una clase ya existe antes de crear

---

### Error #2: CS0111 - Miembro Duplicado en Tipo

**Síntoma:**
```
Type already defines a member called 'Convert' with the same parameter types
```

**Causa raíz:**
- Error derivado del CS0101
- Dos definiciones de `StringToVisibilityConverter` causaban duplicidad de métodos `Convert()` y `ConvertBack()`

**Solución:**
- Resuelta automáticamente al eliminar `StringToVisibilityConverter.cs`
- Ambos errores compartían causa raíz

**Impacto:**
- Bloqueante total
- Dependiente del CS0101

---

## 2. Errores de Arquitectura MVVM

### Error #4: Dependencia Circular HomeViewModel ↔ MainViewModel

**Síntoma:**
```
StackOverflowException al iniciar
'The type 'HomeViewModel' cannot be resolved. 
Dependency cycle detected.'
```

**Causa raíz:**
- `HomeViewModel` recibía `MainViewModel` en constructor para invocar `NavigateToLaunches/Rockets/Map()`
- `MainViewModel` necesitaba inyectar `HomeViewModel`
- Contenedor DI entraba en ciclo infinito:
  ```
  MainViewModel requiere HomeViewModel
    → HomeViewModel requiere MainViewModel
      → MainViewModel requiere HomeViewModel... [CICLO]
  ```

**Solución Aplicada: Method Injection**
```csharp
// HomeViewModel NO recibe MainViewModel
public HomeViewModel(
    Action navigateToLaunches,
    Action navigateToRockets,
    Action navigateToMap)
{
    NavigateToLaunches = navigateToLaunches;
    // ...
}

// MainViewModel construye HomeViewModel con lambdas
_homeViewModel = new HomeViewModel(
    navigateToLaunches: NavigateToLaunches,
    navigateToRockets: NavigateToRockets,
    navigateToMap: NavigateToMap
);

// HomeViewModel NO se registra en DI
// (se construye con 'new', no desde contenedor)
```

**Patrón:** Method Injection / Setter Injection (válido cuando Constructor Injection crea ciclo)

**Impacto:**
- Bloqueante total - aplicación no iniciaba
- Se eliminó también HomeViewModel del registro en `App.xaml.cs`

**Lección:**
- Identificar dependencias circulares tempranamente
- Usar lambdas/Actions para inyectar comportamiento
- No todos los ViewModels deben registrarse en DI

---

### Error #5: GoHomeCommand Ausente en ViewModels Secundarios

**Síntoma:**
- `LaunchesView`, `RocketsView` y `MapView` no tenían botón de retorno al Home
- Usuario quedaba "atrapado" en vistas secundarias sin poder volver

**Causa raíz:**
- DI construye `LaunchesViewModel`, `RocketsViewModel`, `MapViewModel` **antes** de `MainViewModel`
- No podían recibir acción de navegación en constructor sin crear otra dependencia circular

**Solución: Post-Construction Injection**
```csharp
// En cada ViewModel secundario
private Action _navigateToHome;

public void SetNavigateToHome(Action action) => _navigateToHome = action;

[RelayCommand]
public void GoHome() => _navigateToHome?.Invoke();

// En MainViewModel (después de construir VMs)
_launchesViewModel.SetNavigateToHome(NavigateToHome);
_rocketsViewModel.SetNavigateToHome(NavigateToHome);
_mapViewModel.SetNavigateToHome(NavigateToHome);

// En cada View (XAML)
<Button Command="{Binding GoHomeCommand}" Content="← Volver" />
```

**Impacto:**
- Funcionalidad bloqueada - usuario no podía navegar

**Lección:**
- SetNavigateToHome() se llama **después** de construir MainViewModel
- Permite que el ViewModel ya tenga sus métodos NavigateToHome disponibles
- Pattern válido para inyección tardía de dependencias

---

### Error #6: Constructor con Parámetro en LaunchesView

**Síntoma:**
```
'LaunchesView' is not usable as object element. 
No public parameterless constructor found.
```

**Causa raíz:**
- En versión anterior, `LaunchesView` tenía constructor con parámetro
  ```csharp
  public LaunchesView(LaunchesViewModel vm)
  {
      InitializeComponent();
      DataContext = vm;  // Antipatrón
  }
  ```
- WPF instancia `UserControl` vía reflexión **sin parámetros** en `DataTemplate`
- Lanzaba excepción al no encontrar constructor sin parámetros

**Solución Correcta: MVVM Pattern**
```csharp
// Constructor SIEMPRE sin parámetros
public LaunchesView()
{
    InitializeComponent();
    // DataContext llega automáticamente via DataTemplate
}

// En MainWindow.xaml
<DataTemplate DataType="{x:Type vm:LaunchesViewModel}">
    <views:LaunchesView />
</DataTemplate>
```

**Impacto:**
- Bloqueante en la versión del compañero
- Ya estaba resuelto en nuestra versión

**Lección:**
- Views NUNCA deben tener constructor con parámetros
- DataContext se asigna automáticamente via DataTemplate
- Esto es MVVM puro

---

### Error #7: HomeViewModel Registrado en DI Incorrectamente

**Síntoma:**
- `HomeViewModel` se registraba en `App.xaml.cs` como `Singleton`
- Pero `MainViewModel` lo construía con `new`, nunca consultando el contenedor
- Generaba instancia fantasma en memoria

**Causa raíz:**
```csharp
// App.xaml.cs (INCORRECTO)
services.AddSingleton<HomeViewModel>();  // ← Nunca se usa

// MainViewModel (ignora el DI)
_homeViewModel = new HomeViewModel(...);  // ← Usa 'new' en lugar de contenedor
```

**Solución:**
- Eliminar registro de `HomeViewModel` en `ConfigureServices()`
- Documentar con comentario **por qué** no se registra:
  ```csharp
  // HomeViewModel NO se registra aqui porque MainViewModel
  // lo construye internamente pasandole lambdas de navegacion,
  // evitando la dependencia circular (Dependency Inversion Principle)
  ```

**Impacto:**
- No bloqueante en ejecución
- Pero generaba confusión arquitectónica y desperdicio de memoria

**Lección:**
- Documentar decisiones de DI en código
- No registrar VMs que se construyen manualmente
- Mantener consistencia en estrategia de resolución

---

## 3. Errores de Deserialización JSON

### Error #8: PropellantDto Deserializado Incorrectamente como Objeto

**Síntoma:**
```
JsonException: The JSON value could not be converted to PropellantDto. 
Path: $[0].engines.propellant_1
```

**Causa raíz:**
- SpaceX API devuelve `propellant_1` y `propellant_2` como **strings simples**:
  ```json
  {
    "engines": {
      "propellant_1": "liquid oxygen",
      "propellant_2": "RP-1"
    }
  }
  ```
- DTO los tenía definidos como **objetos** `PropellantDto`
  ```csharp
  public class EnginesDto
  {
      public PropellantDto Propellant1 { get; set; }  // Espera objeto
      public PropellantDto Propellant2 { get; set; }
  }
  ```
- `System.Text.Json` no puede deserializar un string JSON en una clase C# con propiedades

**Solución:**
- Cambiar tipo de `Propellant1` y `Propellant2` de `PropellantDto` a **`string`**
  ```csharp
  public class EnginesDto
  {
      public string Propellant1 { get; set; }  //Ahora es string
      public string Propellant2 { get; set; }
  }
  ```
- Eliminar clase `PropellantDto` completa (no se usaba)
- Actualizar mapeo en `SpaceXApiService.MapToRocketModel()`:
  ```csharp
  // Antes:  Propellant1Name = dto.Engines?.Propellant1?.Name  // No existe .Name
  // Después:
  Propellant1Name = dto.Engines?.Propellant1  // Es string
  ```

**Proceso de Diagnóstico:**
1. Activar `Debug > Windows > Exception Settings > Common Language Runtime Exceptions`
2. Ejecutar con F5
3. Visual Studio se detiene en línea exacta: `JsonSerializer.Deserialize<List<RocketDto>>(json)`
4. Exception popup muestra path: `$[0].engines.propellant_1`
5. Con path exacto se identifica DTO problémático

**Impacto:**
- Bloqueante - `GetRocketsAsync()` lanzaba excepción
- `RocketsView` mostraba "Error al obtener cohetes"

**Lección:**
- Inspeccionar JSON real de API antes de diseñar DTOs
- Usar herramientas como Postman para ver estructura JSON
- Los tipos deben coincidir exactamente con API

---

### Error #9: EngineLossMax No Acepta Valor Null

**Síntoma:**
```
JsonException: The JSON value could not be converted to System.Int32. 
Path: $[3].engines.engine_loss_max
```

**Causa raíz:**
- SpaceX API devuelve `engine_loss_max` como `null` para cohetes sin límite (ej: Falcon 1)
- DTO lo definía como **`int`** (no nullable)
  ```csharp
  public int EngineLossMax { get; set; }  // int no puede ser null
  ```
- `System.Text.Json` lanzaba excepción al asignar `null` a `int`

**Solución:**
- Cambiar `EngineLossMax` de `int` a **`int?`** (nullable)
  ```csharp
  public int? EngineLossMax { get; set; }  // Permite null
  ```
- Permite deserializar tanto enteros como `null`

**Impacto:**
- Bloqueante - error en cohete en índice 3 causaba que `GetRocketsAsync()` fallara para **todos** los cohetes

**Lección:**
- Usar nullable types (`?`) cuando API puede devolver `null`
- Anticipar valores `null` en DTOs
- Testear con datos reales, no mocks

---

## 4. Errores de Interfaz de Usuario y Renderizado

### Error #10: Canvas del Mapa con Tamaño Cero

**Síntoma:**
- El área del mapa aparecía **completamente negra** - nada se dibujaba

**Causa raíz:**
- Canvas no tenía `HorizontalAlignment='Stretch'` ni `VerticalAlignment='Stretch'`
- En WPF, un Canvas sin alineación explícita toma tamaño **mínimo (0x0)**
- En `MapView.xaml.cs`, `DrawMap()` verifica:
  ```csharp
  if (width <= 0 || height <= 0) return;  // ← Retorna sin dibujar
  ```

**Solución:**
```xaml
<Canvas x:Name="MapCanvas"
        HorizontalAlignment="Stretch"
        VerticalAlignment="Stretch"
        ClipToBounds="True"
        Background="#060C14" />
```
- `ActualWidth` y `ActualHeight` tienen valores reales
- `DrawMap()` puede dibujar correctamente

**Impacto:**
- Mapa completamente no operativo - pantalla negra sin contenido

---

### Error #11: Mapa Sin Representación Geográfica

**Síntoma:**
- Puntos de lanzamiento flotando sobre fondo negro sin contexto
- Imposible asociar puntos con ubicaciones

**Causa raíz:**
- `DrawMap()` solo dibujaba grilla RGB(40,40,40) casi invisible
- No existía método para continentes
- Puntos sin referencia geográfica visual

**Solución:**
```csharp
private void DrawContinents(double width, double height)
{
    // Implementar 7 continentes como Polygon WPF
    // Coordenadas geográficas reales (lat, lon)
    // Proyección Mercator via GeographicToScreenCoordinates()
    // Colores: océano #060C14, continentes #16305A, borde #28508C
}

private void DrawGrid(double width, double height)
{
    // Mejorar grilla a RGB(30,58,92) para visibilidad
}
```

**Continentes implementados:**
- América del Norte, América del Sur
- Europa, África, Asia
- Australia, Groenlandia

**Impacto:**
- Vista del mapa era ininterpretable

**Lección:**
- La presentación visual es crítica para UX
- Incluir siempre contexto geográfico en mapas
- Colores deben contrastar con fondo

---

### Error #12: Continentes Dibujándose Fuera del Canvas

**Síntoma:**
- Con zoom o pan, polígonos invadían sidebar y otros controles

**Causa raíz:**
- En WPF, hijos de Canvas pueden renderizarse **fuera de sus límites** por diseño
- Al aplicar zoom, coordenadas excedían límites del Canvas

**Solución:**
```xaml
<Canvas ClipToBounds="True" />
```
- Motor de renderizado recorta contenido fuera del rectángulo Canvas
- Evita sobreposición con otros controles

**Impacto:**
- Visual grave - continentes se superponían sobre lista y tabla

---

### Error #13: Todos los Sitios Visibles Automáticamente

**Síntoma:**
- Al navegar a MapView todos los puntos aparecían sin acción del usuario

**Causa raíz:**
- `DrawMap()` iteraba `LaunchSites` completo dibujando punto por cada sitio
- `NavigateToMap()` invocaba `LoadLaunchSitesCommand` automáticamente
- API consultada sin solicitud explícita del usuario

**Solución:**
```csharp
// MapView.xaml.cs DrawMap()
if (ViewModel?.SelectedSite == null) return;  // ← Solo dibuja si hay selección

// MainViewModel NavigateToMap()
public void NavigateToMap()
{
    CurrentViewModel = _mapViewModel;
    // ← NO llamar LoadLaunchSitesCommand automáticamente
}
```

**Flujo correcto:**
1. Entrar al mapa → muestra continentes vacío
2. Presionar "Cargar Sitios" → carga lista
3. Seleccionar sitio en lista → punto aparece

**Impacto:**
- UX incorrecta
- Llamadas API no solicitadas

---

### Error #14: DataGrid con Fondo Blanco en Sidebar del Mapa

**Síntoma:**
- Tabla de lanzamientos del sitio seleccionado con **fondo blanco y texto invisible**

**Causa raíz:**
- Estilo global de DataGrid en `App.xaml` establece `Background='#2a2a2a'`
- Tema WPF sobreescribía `Background` de `DataGridRow` con blanco del sistema
- Sin `RowStyle` explícito, color global no se propaga a filas

**Solución:**
```xaml
<DataGrid>
    <DataGrid.RowStyle>
        <Style TargetType="DataGridRow">
            <Setter Property="Background" Value="#080d14" />
            <Setter Property="Foreground" Value="#c8dff0" />
        </Style>
    </DataGrid.RowStyle>
    <DataGrid.CellStyle>
        <Style TargetType="DataGridCell">
            <Setter Property="Background" Value="Transparent" />
        </Style>
    </DataGrid.CellStyle>
    <Setter Property="IsReadOnly" Value="True" />
</DataGrid>
```

**Impacto:**
- Visual crítico - texto invisible
- Celdas editables incorrectamente en vista de consulta

---

### Error #15: Grilla del Mapa Imperceptible

**Síntoma:**
- Líneas de latitud y longitud invisibles sobre fondo oscuro

**Causa raíz:**
- Color original RGB(40,40,40) casi idéntico al fondo #060C14 = RGB(6,12,20)
- Diferencia de luminosidad insuficiente
- `StrokeThickness 0.5` agravaba invisibilidad

**Solución:**
```csharp
const SolidColorBrush GridColor = new(Color.FromRgb(30, 58, 92));  // Azul marino visible
// StrokeThickness 0.6 (aumentado de 0.5)
```

**Impacto:**
- Visual menor - grilla de referencia inutilizable

---

## 5. Errores de Lógica de Negocio

### Error #16: Filtro "Próximo" No Retorna Resultados

**Síntoma:**
- Al seleccionar "Próximo" en ComboBox el DataGrid quedaba vacío

**Causa raíz:**
- ComboBox tenía `"Proximo"` **sin tilde** (ASCII 111)
- Propiedad calculada `Status` retornaba `"Próximo"` **con tilde** (U+00F3)
- `ApplyFilters()` compara: `if (StatusFilter == "Próximo")`
- `"Proximo" == "Próximo"` es **false** (case-sensitive y diacritic-sensitive)

**Solución:**
- Modificar `Status` en `LaunchModel` para retornar `"Proximo"` **sin tilde**
- Consistente con ComboBox
- Badge en tabla también muestra `"Proximo"` sin tilde como efecto esperado

```csharp
public string Status
{
    get
    {
        if (Success == true) return "Exitoso";
        if (Success == false) return "Fallido";
        return "Proximo";  // ← Sin tilde, consistente
    }
}
```

**Impacto:**
- Funcionalidad de filtrado inoperativa para próximos lanzamientos (subconjunto más relevante)

**Lección:**
- Usar siempre caracteres ASCII en enumeraciones
- Evitar tildes/acentos en strings de comparación
- Testear todos los valores del ComboBox

---

### Error #17: Carga Automática de Sitios al Navegar al Mapa

**Síntoma:**
- Al hacer clic en "Mapa" desde Home, API se consultaba sin solicitud del usuario

**Causa raíz:**
- `NavigateToMap()` incluía llamada a `LoadLaunchSitesCommand.Execute(null)`
- API consultada automáticamente sin que usuario lo solicitara

```csharp
// INCORRECTO
public void NavigateToMap()
{
    CurrentViewModel = _mapViewModel;
    _mapViewModel.LoadLaunchSitesCommand.Execute(null);  // Automático
}
```

**Solución:**
- Eliminar llamada automática
- `NavigateToMap()` solo ejecuta `CurrentViewModel = _mapViewModel`
- Usuario presiona "Cargar Sitios" explícitamente
- Documentar decisión con comentario

```csharp
// CORRECTO
public void NavigateToMap()
{
    // No carga sitios automaticamente.
    // El usuario debe presionar "Cargar Sitios" manualmente.
    CurrentViewModel = _mapViewModel;
}
```

**Impacto:**
- Llamadas innecesarias a API
- UX confusa al ver todos los sitios aparecer automáticamente

**Lección:**
- Nunca hacer llamadas a API sin solicitud explícita del usuario
- Usuario debe tener control sobre cuándo se cargan datos

---

## 6. Decisiones Técnicas Relevantes

### Decisión #1: Elección de Tecnología para el Mapa Interactivo

**Opciones Evaluadas:**

| Opción | Ventajas | Desventajas | Decisión |
|--------|----------|-------------|----------|
| **SVG / Polígonos WPF** | Funciona offline. Sin NuGet extra. Integra con Canvas. Código defendible en defensa. | Mapa simplificado no fotorrealista | ** ELEGIDA** - óptima para proyecto académico |
| **WebBrowser + OpenStreetMap** | Mapa real con tiles. Visualmente superior | IE11 (deprecado). Requiere internet. Comunicación WPF-JS compleja | Descartada - IE11 incompatible |
| **WebView2 + Leaflet** | Mapa real, Chromium moderno. Sin API Key | NuGet WebView2. Requiere internet. Complejidad comunicación | Descartada - complejidad excesiva |
| **WebView2 + Google Maps** | Mapa profesional | API Key con tarjeta crédito. Costo potencial. Complejidad | Descartada - requiere pago |
| **PNG estática** | Implementación rápida (30 min) | Pixelada con zoom. Puntos desalineados | Descartada - calidad insuficiente |

**Razonamiento Final:**
- Proyecto académico sin requerimientos de precisión fotográfica
- Control total sobre código (defendible en defensa)
- Sin dependencias externas (NuGet mínimo)
- Funciona completamente offline
- Implementable en tiempo disponible

---

### Decisión #2: Por Qué Code-Behind de MapView No Viola MVVM

**Cuestionamiento:**
> "MapView.xaml.cs contiene código de dibujo en Canvas. ¿No viola MVVM?"

**Respuesta: NO, porque:**

1. **ViewModel no conoce controles UI**
   - MapViewModel NO tiene referencias a Canvas, Polygon, Ellipse, ni controles WPF
   - No hace `new Polygon()` ni `Canvas.Children.Add()`

2. **Code-behind NO contiene lógica de negocio**
   - DrawMap() no llama API, no filtra datos, no calcula coordenadas
   - NO toma decisiones de negocio

3. **Presentación Pura**
   - DrawMap() toma datos del ViewModel (propiedades) y los convierte en formas visuales
   - Es separación clara: ViewModel proporciona QQString, View decide CÓMO mostrarlo

4. **WPF Sin Binding para Dibujo Dinámico**
   - WPF NO tiene mecanismo nativo de binding para Polygons/Ellipses dinámicos
   - Solución estándar de la industria: code-behind para Canvas
   - Profesionales WPF hacen esto regularmente

5. **Prueba de MVVM:**
   - View podría reemplazarse completamente (ej: WebView2)
   - MapViewModel funcionaría sin cambios
   - Eso demuestra cumplimiento MVVM

**Conclusión:**
- MVVM no significa "cero líneas de code-behind"
- MVVM significa "separación clara de responsabilidades"
- MapView.xaml.cs respeta eso

---

### Decisión #3: Patrón SetNavigateToHome vs Constructor

**Por Qué No Pasar Action en Constructor:**

| Aspecto | Problema |
|--------|----------|
| **Orden de construcción** | DI construye LaunchesViewModel ANTES de MainViewModel |
| **Si constructor recibiera Action** | Necesitaría que MainViewModel existiera primero (otra circular) |
| **Solución: Post-Construction** | SetNavigateToHome() llamado por MainViewModel **después** de ser construido |

**Patrón:**
```csharp
// Paso 1: DI construye LaunchesViewModel (sin acción de navegación)
services.AddSingleton<LaunchesViewModel>();

// Paso 2: DI construye MainViewModel (tiene sus métodos NavigateToHome)
services.AddSingleton<MainViewModel>();

// Paso 3: En MainViewModel constructor (DESPUÉS de existir)
_launchesViewModel.SetNavigateToHome(NavigateToHome);
// ↑ Ahora SetNavigateToHome() existe porque MainViewModel ya está construido
```

**Patrón Formal:** Method Injection / Setter Injection

**Por Qué Es Válido:**
- Técnica estándar cuando Constructor Injection causa ciclo
- No viola MVVM (acción sigue siendo inyectada)
- Explícito y rastreable en código

---

### Decisión #4: Proceso de Diagnóstico de Errores JSON

**Problema:**
> "JsonException en GetRocketsAsync() no era visible en Error List. ¿Cómo lo encontramos?"

**Proceso:**

1. **Activar CLR Exceptions**
   - `Debug > Windows > Exception Settings`
   - Marcar casilla "Common Language Runtime Exceptions"

2. **Reproducir Error**
   - Ejecutar con F5
   - Navegar a RocketsView
   - Presionar "Cargar"

3. **Visual Studio Se Detiene**
   - En línea exacta: `JsonSerializer.Deserialize<List<RocketDto>>(json, _jsonOptions)`
   - Popup de excepción muestra:
     ```
     The JSON value could not be converted to PropellantDto.
     Path: $[0].engines.propellant_1
     ```

4. **Identificar Problema**
   - Path: `$[0].engines.propellant_1`
   - Indica cohete #0, propiedad propellant_1
   - Buscar en RocketDto.cs qué espera

5. **Aplicar Fix**
   - Cambiar Propellant1 de PropellantDto a string

**Lección:**
- No confiar en Error List para errores de runtime
- Common Language Runtime Exceptions es la mejor herramienta
- El path exacto de JsonException es muy útil

---

## 7. Lecciones Aprendidas y Recomendaciones

### Lo que Funcionó Bien

1. **Arquitectura MVVM desde el inicio**
   - Separación clara View/ViewModel/Model
   - Facilité agregar features sin quebrar código
   - Code-behind mínimo

2. **MVVM Toolkit**
   - [ObservableProperty] y [RelayCommand] redujeron boilerplate
   - Evitó errores manuales de PropertyChanged

3. **Inyección de Dependencias**
   - Cambiar implementación de servicio en 1 lugar
   - Testeable desde el inicio

4. **Caché inteligente**
   - Usuarios no notaban latencias
   - API no sobrecargada

### Mejores Prácticas Aplicadas

Testear DTOs con datos REALES de API (no mocks)  
Activar CLR Exceptions para diagnosticar JSON  
Documentar ciclos de dependencia y soluciones  
Usar Post-Construction Injection cuando Constructor causa ciclo  
Code-behind solo para presentación pura  
Nombres sin tildes en enumeraciones/strings de comparación  

---

## Resumen Final

**Total de Errores Encontrados y Resueltos: 17**

| Categoría | Cantidad | Bloqueantes |
|-----------|----------|------------|
| Compilación | 3 | 3 |
| Arquitectura MVVM | 4 | 3 |
| JSON / Deserialización | 2 | 2 |
| UI / Renderizado | 6 | 2 |
| Lógica de Negocio | 2 | 1 |

**Estado Final:**  **Proyecto Compilado sin Errores**

Todas las decisiones técnicas documentadas para referencia futura y defensa académica.

---
