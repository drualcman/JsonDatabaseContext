# JsonDatabaseContext
## Introducci�n
JsonDatabaseContext es una implementaci�n ligera de un "DbContext" similar a Entity Framework, pero que utiliza archivos JSON como almacenamiento persistente. Es ideal para aplicaciones peque�as que necesitan persistencia de datos sin requerir una base de datos completa.

## Caracter�sticas principales
- Persistencia basada en JSON: Almacena datos en archivos JSON legibles
- Operaciones CRUD: Soporta operaciones b�sicas de creaci�n, lectura, actualizaci�n y eliminaci�n
- Thread-safe: Usa sem�foros para garantizar acceso seguro desde m�ltiples hilos
- Simple integraci�n: F�cil de configurar y usar en proyectos .NET

## Configuraci�n
### 1. Agregar la configuraci�n
En tu appsettings.json, agrega la configuraci�n de la base de datos:

```json
{
  "DbContextOptions": {
    "BasePath": "Data/JsonDatabase"
  }
}
```
### 2. Registrar el servicio
En tu archivo de configuraci�n de dependencias (normalmente Program.cs):

```csharp
builder.Services.Configure<DbContextOptions>(builder.Configuration.GetSection(DbContextOptions.SectionKey));
builder.Services.AddScoped<IJsonDbContext, JsonDbContext>();
```
## Uso b�sico
### Obtener un DbSet
Para trabajar con una entidad, primero obt�n su JsonDbSet:

```csharp
var dbSet = await _dbContext.SetAsync<MiEntidad>();
```
### Operaciones CRUD
Crear una entidad
```csharp
var nuevaEntidad = new MiEntidad { /* propiedades */ };
await dbSet.AddAsync(nuevaEntidad);
```
### Leer entidades
```csharp
// Todas las entidades
var todas = dbSet.GetAll();

// Con filtro
var filtradas = dbSet.GetAll(e => e.Propiedad == valor);
Actualizar una entidad
csharp
var entidad = dbSet.GetAll().First();
entidad.Propiedad = nuevoValor;
await dbSet.UpdateAsync(entidad);
Eliminar una entidad
csharp
var entidad = dbSet.GetAll().First();
await dbSet.RemoveAsync(entidad);
Requisitos de las entidades
Todas las entidades deben implementar la interfaz IJsonEntity:

csharp
public class MiEntidad : IJsonEntity
{
    public Guid Id { get; set; }
    // otras propiedades...
}
```
### Estructura de archivos
Los datos se almacenan en archivos JSON en la ruta configurada (BasePath), con un archivo por cada tipo de entidad:

```text
Data/
  JsonDatabase/
    MiEntidad.json
    OtraEntidad.json
```
## Consideraciones
- Rendimiento: Este enfoque es adecuado para conjuntos de datos peque�os. Para datos masivos, considera una base de datos tradicional.
- Concurrencia: Aunque es thread-safe, el acceso masivo concurrente puede afectar el rendimiento.
- Migraciones: No incluye sistema de migraciones. Los cambios en las entidades pueden requerir manipulaci�n manual de los archivos JSON.

## Ejemplo completo
```csharp
// Definici�n de la entidad
public class Producto : IJsonEntity
{
    public Guid Id { get; set; }
    public string Nombre { get; set; }
    public decimal Precio { get; set; }
}

// Uso en un servicio
public class ProductoService
{
    private readonly IJsonDbContext _dbContext;
    
    public ProductoService(IJsonDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    public async Task AgregarProducto(string nombre, decimal precio)
    {
        var productos = await _dbContext.SetAsync<Producto>();
        await productos.AddAsync(new Producto {
            Id = Guid.NewGuid(),
            Nombre = nombre,
            Precio = precio
        });
    }
    
    public async Task<List<Producto>> ObtenerProductosCaros(decimal precioMinimo)
    {
        var productos = await _dbContext.SetAsync<Producto>();
        return productos.GetAll(p => p.Precio >= precioMinimo);
    }
}
```

## Licencia
[MIT License] - Usa libremente en tus proyectos.