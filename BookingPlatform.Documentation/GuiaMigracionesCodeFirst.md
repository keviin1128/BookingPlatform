# Guia de Migraciones a Base de Datos con Code First

Esta guia explica que necesitas para ejecutar migraciones a BD con enfoque Code First usando los modelos de la solucion BookingPlatform.

## 1. Prerequisitos

1. Tener instalado .NET SDK 8 (la solucion apunta a `net8.0`).
2. Tener acceso a una instancia de PostgreSQL.
3. Validar cadena de conexion en `BookingPlatform.API/appsettings.json`:

```json
"Infrastructure": {
  "PostgreSQL": {
    "ConnectionString": "Host=localhost;Port=5432;Database=bookingdb;Username=postgres;Password=root"
  }
}
```

4. Tener disponible la herramienta de EF Core CLI (`dotnet-ef`):

```powershell
dotnet tool install --global dotnet-ef
dotnet ef --version
```

## 2. Elementos de Code First en este proyecto

En esta solucion ya existe la base para Code First:

- `AppDbContext` en `BookingPlatform.Infrastructure/Data/AppDbContext.cs`.
- `DbSet<>` por entidad (`User`, `Worker`, `Service`, `Appointment`, etc.).
- Configuracion Fluent API en `OnModelCreating`.
- Registro de contexto con provider PostgreSQL (`UseNpgsql`) en `BookingPlatform.Infrastructure/ServiceExtensions/InfrastructureExtensions.cs`.
- Inicializacion al arrancar API en `BookingPlatform.API/Program.cs` via `AppDbInitializer.InitializeAsync(...)`.

## 3. Flujo para migrar usando modelos

## Paso A: Cambiar el modelo

1. Edita/crea entidades en `BookingPlatform.Domain/Entities`.
2. Si agregas nueva entidad, registra su `DbSet<>` en `AppDbContext`.
3. Ajusta mapeos y restricciones en `OnModelCreating` (tipos SQL, indices, relaciones, llaves, etc.).

## Paso B: Crear la migracion

Desde la raiz de la solucion (`BookingPlatform`):

```powershell
dotnet ef migrations add NombreMigracion \
  --project BookingPlatform.Infrastructure \
  --startup-project BookingPlatform.API \
  --context AppDbContext \
  --output-dir Data/Migrations
```

Ejemplo:

```powershell
dotnet ef migrations add AddWorkerNotes \
  --project BookingPlatform.Infrastructure \
  --startup-project BookingPlatform.API \
  --context AppDbContext \
  --output-dir Data/Migrations
```

## Paso C: Aplicar la migracion a la BD

```powershell
dotnet ef database update \
  --project BookingPlatform.Infrastructure \
  --startup-project BookingPlatform.API \
  --context AppDbContext
```

## Paso D: Verificar

1. Revisar que se genero la carpeta/archivos de migracion en `BookingPlatform.Infrastructure/Data/Migrations`.
2. Ejecutar la API y validar que no falle el arranque.
3. Ejecutar pruebas:

```powershell
dotnet test BookingPlatform.Tests/BookingPlatform.Tests.csproj
```

## 4. Comandos utiles

Listar migraciones:

```powershell
dotnet ef migrations list \
  --project BookingPlatform.Infrastructure \
  --startup-project BookingPlatform.API \
  --context AppDbContext
```

Eliminar ultima migracion (si aun no fue aplicada o quieres regenerarla):

```powershell
dotnet ef migrations remove \
  --project BookingPlatform.Infrastructure \
  --startup-project BookingPlatform.API \
  --context AppDbContext
```

Generar script SQL (para despliegues controlados):

```powershell
dotnet ef migrations script \
  --project BookingPlatform.Infrastructure \
  --startup-project BookingPlatform.API \
  --context AppDbContext \
  --output migration.sql
```

## 5. Recomendaciones de equipo

1. Usar nombres claros de migracion (ejemplo: `AddLoyaltyRewardExpiration`).
2. No mezclar cambios de multiples features en una sola migracion.
3. Versionar siempre los archivos de migracion en Git.
4. Revisar cuidadosamente operaciones destructivas (drop/rename) antes de ejecutar en ambientes compartidos.
5. En produccion, preferir script SQL revisado antes de aplicar directo por CLI.

## 6. Nota importante del comportamiento actual

`AppDbInitializer` aplica automaticamente migraciones al iniciar la API cuando detecta migraciones definidas (`Database.MigrateAsync`).

Esto facilita desarrollo local, pero en entornos productivos se recomienda controlar la ejecucion via pipeline/deploy y script SQL.
