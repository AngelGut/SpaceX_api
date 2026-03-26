# 🚀 SpaceX Explorer

**Aplicación WPF moderna para explorar datos en tiempo real de SpaceX.**

Desarrollada con **.NET 8**, **MVVM Toolkit**, **Inyección de Dependencias**, **Principios SOLID** y **Arquitectura en capas**.

## 🎯 ¿Qué Hace?
```
**SpaceX Explorer** es una aplicación que trae datos en tiempo real de SpaceX y los muestra de forma visual:

- **🛸 Lanzamientos**: Historial completo con búsqueda y filtros por estado
- **🚀 Cohetes**: Especificaciones técnicas (altura, masa, costo, tasa de éxito)
- **🌍 Mapa**: Sitios de lanzamiento interactivos con zoom y pan
- **🏠 Menú**: Navegación intuitiva entre secciones

**Consumo de API**: SpaceX API v4 → Mapeo a Models → ViewModels → UI WPF
```

## ⚡ Inicio Rápido

```bash
# 1. Clonar el repositorio
git clone <tu-repo>
cd SpaceXApp_v3

# 2. Abrir SpaceXApp.csproj en Visual Studio 2022
# 3. Visual Studio restaura NuGet automáticamente
```
## 📚 Documentación

- **[ARQUITECTURA.md](docs/ARQUITECTURA.md)** - Explicación detallada de SOLID + capas
- **[GUIA_DESARROLLO.md](docs/GUIA_DESARROLLO.md)** - Cómo extender el proyecto
- **[API_REFERENCE.md](docs/API_REFERENCE.md)** - Referencia de métodos


## 🎓 Para el Equipo

1. **Primero**: Leer `README.md` (esto)
2. **Luego**: Leer `docs/ARQUITECTURA.md`
3. **Después**: Leer `docs/GUIA_DESARROLLO.md`
4. **Finalmente**: Revisar `docs/API_REFERENCE.md`

## 💡 Tips para Trabajo en Equipo

✅ **Antes de empezar**:
- `git pull origin develop` (traer cambios de otros)

✅ **Al terminar tu feature**:
- `git push origin feature/xxxxx`
- Hacer Pull Request a `develop`
- Esperar revisión del líder

✅ **Si hay conflictos**:
- Comunicarse con el líder
- Resolver conflictos juntos
- Validar que no se rompa nada
