# Calendar Backend

Comandos rapidos para levantar y validar el backend de `calendar_manager`.

## Requisitos

- .NET 10 SDK.
- Docker con Docker Compose.
- Ejecutar los comandos desde esta carpeta: `backend/`.

## Preparacion inicial

1. Copia las variables locales si no existe `.env`:

```bash
cp .env.example .env
```

2. Restaura las herramientas locales de .NET:

```bash
dotnet tool restore
```

## Levantar todo en desarrollo

Usa el script de desarrollo:

```bash
bash ./start-dev.sh
```

El script levanta primero SQL Server con Docker Compose y despues arranca la API con el perfil HTTP en `http://localhost:5167`.

## Comandos utiles

Levantar solo SQL Server:

```bash
docker compose up -d --wait
```

Aplicar migraciones:

```bash
dotnet tool run dotnet-ef database update --project src/Calendar.Infrastructure --startup-project src/Calendar.Api --context CalendarDbContext
```

Crear una migracion nueva:

```bash
dotnet tool run dotnet-ef migrations add <Name> --project src/Calendar.Infrastructure --startup-project src/Calendar.Api --context CalendarDbContext --output-dir Persistence/Migrations
```

Ejecutar solo la API:

```bash
dotnet run --project src/Calendar.Api --launch-profile http
```

Compilar:

```bash
dotnet build
```

Health check:

```bash
curl http://localhost:5167/health
```

## Detener infraestructura local

```bash
docker compose down
```

Para borrar tambien el volumen de SQL Server:

```bash
docker compose down -v
```
