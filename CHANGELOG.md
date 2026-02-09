# Changelog

Todos los cambios notables en este proyecto serán documentados en este archivo.

El formato se basa en [Keep a Changelog](https://keepachangelog.com/es-ES/1.0.0/),
y este proyecto adhiere a [Semantic Versioning](https://semver.org/lang/es/).

## [Unreleased]

## [1.0.0] - 2024-01-15

### Added
- Implementación inicial de `JsonDbContext` con soporte para .NET 9 y .NET 10
- Interfaz `IJsonDbContext` para inyección de dependencias
- Interfaz `IJsonEntity` para definir entidades persistibles
- Clase `JsonDbSet<T>` con operaciones CRUD completas:
  - `GetAllAsync()` - Obtener todas las entidades
  - `AddAsync(T entity)` - Agregar una entidad
  - `AddRangeAsync(IEnumerable<T> entities)` - Agregar múltiples entidades
  - `UpdateAsync(T entity)` - Actualizar una entidad
  - `UpdateRangeAsync(IEnumerable<T> entities)` - Actualizar múltiples entidades
  - `RemoveAsync(T entity)` - Eliminar una entidad
  - `RemoveRangeAsync(IEnumerable<T> entities)` - Eliminar múltiples entidades
- Generación automática de IDs (Guid) para nuevas entidades
- Soporte para sincronización con semáforos para operaciones concurrentes
- Almacenamiento de datos en archivos JSON
- Documentación completa con ejemplos de uso
- Patrón Repository genérico
- Ejemplos de configuración en `Program.cs`
- Controlador ASP.NET Core de ejemplo
- Soporte para SourceLink para mejor debugging

### Features
- Concurrencia segura con `SemaphoreSlim` por tipo de entidad
- Rutas configurables para almacenamiento de datos
- Soporte para múltiples frameworks (.NET 9 y .NET 10)
- XML documentation para IntelliSense
- Símbolos de depuración (snupkg)

[Unreleased]: https://github.com/drualcman/JsonDatabaseContext/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/drualcman/JsonDatabaseContext/releases/tag/v1.0.0
